import { render, screen } from '@testing-library/react-native';
import { theme } from '@/shared/theme/tokens';
import { AppHeader, SubHeader } from '@/shared/ui-kit';
import { ScreenRenderer, type ScreenConfig } from '@/shared/screen-config';

describe('Header and SubHeader CSS & Padding Separation', () => {
  it('AppHeader có padding riêng và giữ nguyên paddingTop mặc định (5px)', async () => {
    await render(<AppHeader />);

    const brandText = screen.getByText('GymBase');
    const headerContainer = brandText.parent?.parent;
    expect(headerContainer).toBeTruthy();

    const flatStyle = Object.assign(
      {},
      ...[headerContainer?.props.style].flat().filter(Boolean),
    );
    expect(flatStyle.paddingTop).toBe(theme.spacing.sm - 3); // 5px
    expect(flatStyle.paddingHorizontal).toBe(theme.spacing.lg - 2); // 14px
  });

  it('SubHeader có style riêng và có paddingTop (16px) lớn hơn paddingTop của AppHeader', async () => {
    await render(<SubHeader title="Đăng bài" />);

    const titleElement = screen.getByText('Đăng bài');
    const subHeaderContainer = titleElement.parent;
    expect(subHeaderContainer).toBeTruthy();

    const flatStyle = Object.assign(
      {},
      ...[subHeaderContainer?.props.style].flat().filter(Boolean),
    );
    // paddingTop của SubHeader đã được tăng riêng lên 16px (theme.spacing.md + 4)
    expect(flatStyle.paddingTop).toBe(theme.spacing.md + 4);
    expect(flatStyle.paddingTop).toBeGreaterThan(theme.spacing.sm - 3);
  });

  it('cho phép tùy biến style riêng (ví dụ tăng paddingTop) qua prop style', async () => {
    // 1. AppHeader nhận custom style
    await render(<AppHeader style={{ paddingTop: 10, backgroundColor: '#ffffff' }} />);
    const brand = screen.getByText('GymBase');
    const headerContainer = brand.parent?.parent;
    const headerStyle = Object.assign(
      {},
      ...[headerContainer?.props.style].flat().filter(Boolean),
    );
    expect(headerStyle.paddingTop).toBe(10);
    expect(headerStyle.backgroundColor).toBe('#ffffff');

    // 2. SubHeader nhận custom style
    await render(<SubHeader title="Tùy chỉnh" style={{ paddingTop: 28 }} />);
    const title = screen.getByText('Tùy chỉnh');
    const subHeaderContainer = title.parent;
    const subHeaderStyle = Object.assign(
      {},
      ...[subHeaderContainer?.props.style].flat().filter(Boolean),
    );
    expect(subHeaderStyle.paddingTop).toBe(28);
  });

  it('ScreenRenderer truyền style riêng của header và subHeader độc lập', async () => {
    const config: ScreenConfig = {
      id: 'test-styles',
      header: {
        style: { paddingTop: 8 },
      },
      subHeader: {
        title: 'Màn hình thử nghiệm',
        style: { paddingTop: 24 },
      },
      sections: [],
      bottomTabs: [],
    };

    await render(<ScreenRenderer config={config} />);

    const brand = screen.getByText('GymBase');
    const headerBox = brand.parent?.parent;
    const headerStyle = Object.assign({}, ...[headerBox?.props.style].flat().filter(Boolean));
    expect(headerStyle.paddingTop).toBe(8);

    const subTitle = screen.getByText('Màn hình thử nghiệm');
    const subBox = subTitle.parent;
    const subStyle = Object.assign({}, ...[subBox?.props.style].flat().filter(Boolean));
    expect(subStyle.paddingTop).toBe(24);
  });
});
