# Tham khảo Cấu hình AWS S3 + CloudFront (OAC) & Terraform

Tài liệu này cung cấp mẫu cấu hình chuẩn Production cho hạ tầng lưu trữ ảnh GymBase.

---

## 1. Mẫu Terraform (HCL)

```hcl
terraform {
  required_version = ">= 1.5.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

variable "environment" {
  type    = string
  default = "staging"
}

variable "bucket_name" {
  type    = string
  default = "gymbase-media-staging"
}

# 1. Private S3 Bucket
resource "aws_s3_bucket" "media" {
  bucket = var.bucket_name

  tags = {
    Project     = "GymBase"
    Environment = var.environment
  }
}

# 2. Chặn toàn bộ Public Access (Security Best Practice)
resource "aws_s3_bucket_public_access_block" "media_pab" {
  bucket = aws_s3_bucket.media.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# 3. Cấu hình CORS (Cho phép Mobile/Web upload trực tiếp qua Presigned PUT URL)
resource "aws_s3_bucket_cors_configuration" "media_cors" {
  bucket = aws_s3_bucket.media.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["PUT", "GET", "HEAD"]
    allowed_origins = ["*"] # Production: có thể thu hẹp vào domain web nếu cần
    expose_headers  = ["ETag"]
    max_age_seconds = 3000
  }
}

# 4. Vòng đời S3 (Lifecycle Rule): Dọn dẹp staging/ sau 1 ngày
resource "aws_s3_bucket_lifecycle_configuration" "media_lifecycle" {
  bucket = aws_s3_bucket.media.id

  rule {
    id     = "delete-abandoned-staging-uploads"
    status = "Enabled"

    filter {
      prefix = "staging/"
    }

    expiration {
      days = 1
    }
  }

  rule {
    id     = "progress-photos-tiering"
    status = "Enabled"

    filter {
      prefix = "media/progress_photo/"
    }

    transition {
      days          = 30
      storage_class = "INTELLIGENT_TIERING"
    }
  }
}

# 5. CloudFront Origin Access Control (OAC)
resource "aws_cloudfront_origin_access_control" "media_oac" {
  name                              = "gymbase-media-oac"
  description                       = "OAC for GymBase Private S3 Bucket"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# 6. CloudFront Distribution
resource "aws_cloudfront_distribution" "media_cdn" {
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "GymBase Media CDN"
  default_root_object = ""

  origin {
    domain_name              = aws_s3_bucket.media.bucket_regional_domain_name
    origin_id                = "S3-GymBase-Media"
    origin_access_control_id = aws_cloudfront_origin_access_control.media_oac.id
  }

  default_cache_behavior {
    allowed_methods        = ["GET", "HEAD", "OPTIONS"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = "S3-GymBase-Media"
    viewer_protocol_policy = "redirect-to-https"

    forwarded_values {
      query_string = false
      headers      = ["Origin"]

      cookies {
        forward = "none"
      }
    }

    min_ttl     = 0
    default_ttl = 86400
    max_ttl     = 31536000
    compress    = true
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = true
  }
}

# 7. Bucket Policy cho phép CloudFront đọc S3 qua OAC
resource "aws_s3_bucket_policy" "media_bucket_policy" {
  bucket = aws_s3_bucket.media.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "AllowCloudFrontServicePrincipalReadOnly"
        Effect    = "Allow"
        Principal = {
          Service = "cloudfront.amazonaws.com"
        }
        Action   = "s3:GetObject"
        Resource = "${aws_s3_bucket.media.arn}/*"
        Condition = {
          StringEquals = {
            "AWS:SourceArn" = aws_cloudfront_distribution.media_cdn.arn
          }
        }
      }
    ]
  })
}

output "s3_bucket_name" {
  value = aws_s3_bucket.media.id
}

output "cloudfront_domain_name" {
  value = aws_cloudfront_distribution.media_cdn.domain_name
}
```

---

## 2. AWS CLI Tương đương

### 1. Tạo Bucket Private & Chặn Public Access:
```bash
aws s3api create-bucket \
    --bucket gymbase-media \
    --region ap-southeast-1 \
    --create-bucket-configuration LocationConstraint=ap-southeast-1

aws s3api put-public-access-block \
    --bucket gymbase-media \
    --public-access-block-configuration "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"
```

### 2. Cấu hình CORS (Cho phép Mobile PUT):
```json
// cors.json
{
  "CORSRules": [
    {
      "AllowedHeaders": ["*"],
      "AllowedMethods": ["PUT", "GET", "HEAD"],
      "AllowedOrigins": ["*"],
      "ExposeHeaders": ["ETag"],
      "MaxAgeSeconds": 3000
    }
  ]
}
```
```bash
aws s3api put-bucket-cors --bucket gymbase-media --cors-configuration file://cors.json
```

### 3. Cấu hình Lifecycle Rule (Tự động xóa file staging mồ côi sau 1 ngày):
```json
// lifecycle.json
{
  "Rules": [
    {
      "ID": "expire-staging-uploads",
      "Status": "Enabled",
      "Filter": {
        "Prefix": "staging/"
      },
      "Expiration": {
        "Days": 1
      }
    }
  ]
}
```
```bash
aws s3api put-bucket-lifecycle-configuration --bucket gymbase-media --lifecycle-configuration file://lifecycle.json
```
