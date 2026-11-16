import { render, screen } from '@testing-library/react-native';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage, resolveRemoteImageUri } from '@/shared/ui-kit/remote-image';

describe('resolveRemoteImageUri', () => {
  it('giữ nguyên URL tuyệt đối (http, https, file, data, blob)', () => {
    expect(resolveRemoteImageUri('https://cdn.gymbase.vn/cat.webp')).toBe('https://cdn.gymbase.vn/cat.webp');
    expect(resolveRemoteImageUri('http://localhost:5020/cat.webp')).toBe('http://localhost:5020/cat.webp');
    expect(resolveRemoteImageUri('file:///local/photo.jpg')).toBe('file:///local/photo.jpg');
    expect(resolveRemoteImageUri('data:image/webp;base64,abc')).toBe('data:image/webp;base64,abc');
  });

  it('ghép host base khi là relative path', () => {
    const res = resolveRemoteImageUri('/api/v1/mob/files/123/content');
    expect(res).toBeDefined();
    expect(res).toContain('/api/v1/mob/files/123/content');
  });

  it('trả về undefined khi uri rỗng hoặc null', () => {
    expect(resolveRemoteImageUri(undefined)).toBeUndefined();
    expect(resolveRemoteImageUri('')).toBeUndefined();
    expect(resolveRemoteImageUri('   ')).toBeUndefined();
  });
});

describe('RemoteImage Component', () => {
  it('hiển thị hình tròn hoàn hảo khi circle = true (cả khi có ảnh và khi fallback)', async () => {
    // 1. Trường hợp có URI: Image có borderRadius bo tròn
    await render(
      <RemoteImage
        uri="https://cdn.gymbase.vn/avatar.webp"
        accessibilityLabel="Avatar tròn"
        circle
        style={{ width: 36, height: 36 }}
      />,
    );

    const image = screen.getByLabelText('Avatar tròn');
    expect(image).toBeTruthy();
    // Kiểm tra style có borderRadius của chip (999)
    const flatStyle = Object.assign({}, ...[image.props.style].flat().filter(Boolean));
    expect(flatStyle.borderRadius).toBe(theme.radius.chip);

    // 2. Trường hợp fallback (không có URI): View fallback có borderRadius bo tròn
    await render(
      <RemoteImage
        uri=""
        accessibilityLabel="Fallback tròn"
        circle
        style={{ width: 36, height: 36 }}
      />,
    );

    const fallback = screen.getByLabelText('Fallback tròn');
    expect(fallback).toBeTruthy();
    const fallbackFlatStyle = Object.assign({}, ...[fallback.props.style].flat().filter(Boolean));
    expect(fallbackFlatStyle.borderRadius).toBe(theme.radius.chip);
  });
});
