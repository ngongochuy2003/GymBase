/** Chỉ nhận link `https` có tên miền, không có khoảng trắng (adr.md, N41). */
export function laLinkHopLe(link: string | undefined): link is string {
  return /^https:\/\/[^\s/]+\.[^\s/]+(\/\S*)?$/i.test(link?.trim() ?? '');
}
