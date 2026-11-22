import type {
  AnhCoTheMoc,
  ChiTietQuanLy,
  CheckInQuanLy,
  KeHoachLoTrinh,
  LoTrinhCuaToi,
  LoTrinhQuanLy,
  NguoiLoTrinh,
  NhiemVuNhom,
  ThanhVienLoTrinh,
  TienDoLoTrinh,
} from '../types';
import { layNgayLech, tuanHienTaiCua } from '../utils/ngay';

const anh = (n: number) => `https://i.pravatar.cc/200?img=${n}`;
const bia = (ten: string) => `https://picsum.photos/seed/${ten}/400/520`;

const nguoi = (id: string, ten: string, imgId: number): NguoiLoTrinh => ({
  id,
  ten,
  anh: anh(imgId),
});

const ptTranDucAnh = nguoi('pt-1', 'Trần Đức Anh', 33);

/**
 * Dữ liệu mẫu cho màn Lộ trình; ngày tính theo `homNay` để màn luôn có dữ liệu ở tháng hiện tại.
 */
// TODO(ROUTE): thay bằng API lộ trình (danh sách của tôi, danh sách tôi quản lý) khi backend có bảng lộ trình; bỏ file seed này.
export function taoDuLieuMau(homNay: Date): {
  cuaToi: LoTrinhCuaToi[];
  quanLy: LoTrinhQuanLy[];
} {
  const cuaToi: LoTrinhCuaToi[] = [
    {
      id: 'ct-1',
      ten: 'Tăng 10 kg trong 3 tháng',
      loai: 'don',
      trangThai: 'dang_hoat_dong',
      ngayBatDau: layNgayLech(homNay, -28),
      ngayKetThuc: layNgayLech(homNay, 56),
      tongTuan: 12,
      tienDo: 42,
      anhBia: bia('tang-co'),
      pt: ptTranDucAnh,
      tuanHienTai: 5,
      mucTieu: { tu: 60, den: 70, donVi: 'kg' },
      tuanThu: 92,
      checkInTiepTheo: 'Ngày mai, 08:00',
      nhiemVuHomNay: [
        {
          id: 'nv-7',
          loai: 'buoi_tap',
          tieuDe: 'Buổi tập – 06:00',
          moTa: 'Cardio & Core',
          trangThai: 'da_check',
          buoiTap: {
            keHoach: {
              ten: 'Cardio & Core',
              gio: '06:00',
              nhomCo: 'Tim mạch – Bụng',
              thoiLuongPhut: 30,
              anh: bia('cardio-core'),
              baiTap: [
                { id: 'bt-5', ten: 'Chạy bộ nhẹ', soSet: 1, soLan: '20 phút' },
                { id: 'bt-6', ten: 'Plank', soSet: 3, soLan: '45 giây' },
                { id: 'bt-7', ten: 'Crunch', soSet: 3, soLan: '15 rep' },
              ],
            },
            checkIn: {
              trangThaiBuoi: 'da_tap',
              baiTapXong: ['bt-5', 'bt-6', 'bt-7'],
              ghiChu: 'Chạy đủ 20 phút, nhịp tim ổn định.',
              trangThaiDuyet: 'da_duyet',
              gioGui: '06:40',
              danhGia: {
                pt: ptTranDucAnh,
                noiDung: 'Buổi sáng làm đủ bài, giữ nhịp này nhé!',
                thoiDiem: 'Hôm nay, 07:05',
              },
            },
          },
        },
        {
          id: 'nv-1',
          loai: 'bua_an',
          tieuDe: 'Bữa sáng – 07:00',
          moTa: '≈ 550 kcal',
          trangThai: 'da_check',
          buaAn: {
            keHoach: {
              ten: 'Bữa sáng',
              gio: '07:00',
              moTa: 'Yến mạch sữa chua, 2 trứng luộc, 1 quả chuối',
              kcal: 550,
              anh: bia('bua-sang'),
            },
            checkIn: {
              trangThaiBua: 'da_an',
              anh: [bia('bua-sang-thuc-te')],
              ghiChu: 'Ăn đủ như kế hoạch, thêm một ly sữa không đường.',
              trangThaiDuyet: 'da_duyet',
              gioGui: '07:25',
              danhGia: {
                pt: ptTranDucAnh,
                noiDung: 'Bữa sáng đủ đạm và tinh bột, rất tốt. Giữ nhịp này nhé!',
                thoiDiem: 'Hôm nay, 08:10',
              },
            },
          },
        },
        {
          id: 'nv-2',
          loai: 'bua_an',
          tieuDe: 'Bữa trưa – 12:00',
          moTa: '≈ 700 kcal',
          trangThai: 'khong_hoan_thanh',
          buaAn: {
            keHoach: {
              ten: 'Bữa trưa',
              gio: '12:00',
              moTa: 'Cơm gạo lứt, ức gà nướng, rau luộc',
              kcal: 700,
              anh: bia('bua-trua'),
            },
            checkIn: {
              trangThaiBua: 'bo_bua',
              anh: [],
              ghiChu: 'Họp đột xuất kéo dài nên mình bỏ bữa trưa.',
              trangThaiDuyet: 'cho_duyet',
              gioGui: '13:10',
            },
          },
        },
        {
          id: 'nv-3',
          loai: 'buoi_tap',
          tieuDe: 'Buổi tập – 17:30',
          moTa: 'Upper A',
          trangThai: 'check_in_ngay',
          buoiTap: {
            keHoach: {
              ten: 'Upper A',
              gio: '17:30',
              nhomCo: 'Ngực – Vai – Tay sau',
              thoiLuongPhut: 60,
              anh: bia('upper-a'),
              baiTap: [
                { id: 'bt-1', ten: 'Bench Press', soSet: 4, soLan: '8–10 rep' },
                { id: 'bt-2', ten: 'Incline Dumbbell Press', soSet: 4, soLan: '10–12 rep' },
                { id: 'bt-3', ten: 'Shoulder Press', soSet: 3, soLan: '10 rep' },
                { id: 'bt-4', ten: 'Triceps Pushdown', soSet: 3, soLan: '12 rep' },
              ],
            },
          },
        },
        {
          id: 'nv-6',
          loai: 'bua_an',
          tieuDe: 'Bữa tối – 19:00',
          moTa: '≈ 650 kcal',
          trangThai: 'check_in_ngay',
          buaAn: {
            keHoach: {
              ten: 'Bữa tối',
              gio: '19:00',
              moTa: 'Khoai lang nướng, cá hồi áp chảo, salad rau xanh',
              kcal: 650,
              anh: bia('bua-toi'),
            },
          },
        },
        {
          id: 'nv-4',
          loai: 'can_nang',
          tieuDe: 'Cân nặng – Tuần 5',
          moTa: 'Cập nhật 1 lần/tuần',
          trangThai: 'check_in_ngay',
        },
        {
          id: 'nv-5',
          loai: 'anh_co_the',
          tieuDe: 'Ảnh cơ thể – Tuần 5',
          moTa: 'So sánh tiến độ',
          trangThai: 'check_in_ngay',
        },
      ],
    },
    {
      id: 'ct-2',
      ten: 'Giảm mỡ bụng 8 tuần',
      loai: 'don',
      trangThai: 'hoan_thanh',
      ngayBatDau: layNgayLech(homNay, -120),
      ngayKetThuc: layNgayLech(homNay, -64),
      tongTuan: 8,
      tienDo: 100,
      anhBia: bia('giam-mo'),
      pt: nguoi('pt-2', 'Lê Minh Châu', 47),
      tuanHienTai: 8,
      mucTieu: { tu: 78, den: 72, donVi: 'kg' },
      tuanThu: 88,
      checkInTiepTheo: 'Đã kết thúc',
      nhiemVuHomNay: [],
    },
    {
      id: 'ct-3',
      ten: 'Chạy bộ 5 km không nghỉ',
      loai: 'nhom',
      trangThai: 'that_bai',
      ngayBatDau: layNgayLech(homNay, -90),
      ngayKetThuc: layNgayLech(homNay, -34),
      tongTuan: 8,
      tienDo: 35,
      anhBia: bia('chay-bo'),
      pt: nguoi('pt-3', 'Phạm Quang Huy', 15),
      tuanHienTai: 4,
      mucTieu: { tu: 0, den: 5, donVi: 'km' },
      tuanThu: 51,
      checkInTiepTheo: 'Đã kết thúc',
      nhiemVuHomNay: [],
    },
  ];

  const quanLy: LoTrinhQuanLy[] = [
    {
      id: 'ql-1',
      ten: 'Tăng 10 kg trong 3 tháng',
      loai: 'don',
      trangThai: 'dang_hoat_dong',
      ngayBatDau: layNgayLech(homNay, -28),
      ngayKetThuc: layNgayLech(homNay, 56),
      tongTuan: 12,
      tienDo: 42,
      anhBia: bia('hv-minh'),
      nguoiThamGia: [nguoi('hv-1', 'Nguyễn Văn Minh', 12)],
      soCheckInChoDuyet: 2,
      soBuoiTapHomNay: 1,
      nhiemVuTiepTheo: {
        loai: 'buoi_tap',
        chiTiet: 'Buổi tập Upper A (hôm nay, 17:30)',
        quaHan: false,
      },
    },
    {
      id: 'ql-2',
      ten: 'Giảm 6 kg mỡ trong 8 tuần',
      loai: 'don',
      trangThai: 'dang_hoat_dong',
      ngayBatDau: layNgayLech(homNay, -49),
      ngayKetThuc: layNgayLech(homNay, 5),
      tongTuan: 8,
      tienDo: 88,
      anhBia: bia('hv-lan'),
      nguoiThamGia: [nguoi('hv-2', 'Trần Thị Lan', 45)],
      soCheckInChoDuyet: 0,
      soBuoiTapHomNay: 0,
      nhiemVuTiepTheo: {
        loai: 'bua_an',
        chiTiet: 'Check-in bữa sáng (quá hạn 2 ngày)',
        quaHan: true,
      },
    },
    {
      id: 'ql-3',
      ten: 'Nhóm Săn chắc cơ thể K12',
      loai: 'nhom',
      trangThai: 'dang_hoat_dong',
      ngayBatDau: layNgayLech(homNay, -14),
      ngayKetThuc: layNgayLech(homNay, 70),
      tongTuan: 12,
      tienDo: 18,
      anhBia: bia('nhom-k12'),
      nguoiThamGia: [
        nguoi('hv-3', 'Lê Hoàng Nam', 8),
        nguoi('hv-4', 'Phạm Thu Hà', 44),
        nguoi('hv-5', 'Đỗ Quốc Bảo', 52),
        nguoi('hv-6', 'Vũ Khánh Linh', 49),
        nguoi('hv-7', 'Ngô Tuấn Kiệt', 60),
      ],
      soCheckInChoDuyet: 5,
      soBuoiTapHomNay: 1,
      nhiemVuTiepTheo: {
        loai: 'buoi_tap',
        chiTiet: 'Buổi tập Chân (hôm nay, 18:00)',
        quaHan: false,
      },
    },
    {
      id: 'ql-4',
      ten: 'Tăng sức bền cho người mới',
      loai: 'don',
      trangThai: 'hoan_thanh',
      ngayBatDau: layNgayLech(homNay, -70),
      ngayKetThuc: layNgayLech(homNay, -14),
      tongTuan: 8,
      tienDo: 100,
      anhBia: bia('hv-phuc'),
      nguoiThamGia: [nguoi('hv-8', 'Hoàng Gia Phúc', 11)],
      soCheckInChoDuyet: 0,
      soBuoiTapHomNay: 0,
    },
    {
      id: 'ql-5',
      ten: 'Giảm cân sau sinh',
      loai: 'don',
      trangThai: 'that_bai',
      ngayBatDau: layNgayLech(homNay, -40),
      ngayKetThuc: layNgayLech(homNay, 16),
      tongTuan: 8,
      tienDo: 30,
      anhBia: bia('hv-mai'),
      nguoiThamGia: [nguoi('hv-9', 'Bùi Thanh Mai', 48)],
      soCheckInChoDuyet: 0,
      soBuoiTapHomNay: 0,
      lyDoDong: 'Học viên nghỉ tập do bận công việc, hai bên thống nhất dừng lộ trình.',
    },
  ];

  return { cuaToi, quanLy };
}

/**
 * Danh sách bạn của PT để mời vào lộ trình (form tạo lộ trình).
 */
// TODO(ROUTE): lấy từ API danh sách bạn, chỉ người đã kết bạn với PT, tải dần và phân trang (adr.md, N25, N26).
export function taoBanBeMau(): NguoiLoTrinh[] {
  return [
    nguoi('hv-10', 'Minh Khôi', 12),
    nguoi('hv-1', 'Nguyễn Văn Minh', 14),
    nguoi('hv-2', 'Trần Thị Lan', 45),
    nguoi('hv-3', 'Lê Hoàng Nam', 8),
    nguoi('hv-4', 'Phạm Thu Hà', 44),
    nguoi('hv-5', 'Đỗ Quốc Bảo', 52),
    nguoi('hv-6', 'Vũ Khánh Linh', 49),
    nguoi('hv-7', 'Ngô Tuấn Kiệt', 60),
  ];
}

/** Số lần cân mỗi tuần trong dữ liệu mẫu. */
const LAN_CAN_MOI_TUAN = 2;

/** Tỉ lệ quãng đường đã đi tới mục tiêu ở từng lần cân (8 tuần x 2 lần), có vài lần tụt nhẹ cho giống thật. */
const TI_LE_CAN_NANG = [
  0, 0.03, 0.08, 0.07, 0.17, 0.2, 0.29, 0.33, 0.4, 0.43, 0.48, 0.47, 0.59, 0.61, 0.65, 0.65,
];

/** Ảnh cơ thể chụp cách nhau 2 tuần, bắt đầu từ Tuần 1 và luôn có mốc của tuần hiện tại. Mốc đầu cho PT xem, các mốc sau riêng tư. */
function taoMocAnhMau(lo: Pick<LoTrinhCuaToi, 'id' | 'tuanHienTai'>): AnhCoTheMoc[] {
  const tuan: number[] = [];
  for (let t = 1; t < lo.tuanHienTai; t += 2) tuan.push(t);
  tuan.push(lo.tuanHienTai);
  return tuan.map((t, i) => ({
    tuan: t,
    uri: bia(`${lo.id}-tuan-${t}`),
    quyenXem: i === 0 ? 'cho_pt' : 'rieng_tu',
  }));
}

/** Số liệu tiến độ mẫu cho màn chi tiết lộ trình, suy ra từ mục tiêu của lộ trình. */
// TODO(ROUTE): thay bằng API tiến độ (cân nặng theo tuần, check-in đã duyệt, ảnh theo mốc, đánh giá mới nhất của PT).
export function taoTienDoMau(
  lo: Pick<LoTrinhCuaToi, 'id' | 'mucTieu' | 'tuanHienTai' | 'tienDo' | 'pt'>,
): TienDoLoTrinh {
  const quang = lo.mucTieu.den - lo.mucTieu.tu;
  return {
    canNang: TI_LE_CAN_NANG.map((tiLe, i) => ({
      tuan: Math.floor(i / LAN_CAN_MOI_TUAN) + 1,
      lan: (i % LAN_CAN_MOI_TUAN) + 1,
      kg: Math.round((lo.mucTieu.tu + quang * tiLe) * 10) / 10,
    })),
    buoiTap: { xong: 14, tong: 16 },
    buaAn: { xong: 24, tong: 30 },
    anhMoc: taoMocAnhMau(lo),
    danhGia: {
      pt: lo.pt,
      noiDung: `Tiến độ rất tốt! Bạn đã đi được ${lo.tienDo}% chặng đường, bám sát lộ trình. Hãy tiếp tục duy trì lịch tập và ăn đủ lượng theo kế hoạch. Cố lên! 💪`,
      thoiDiem: 'Hôm nay',
    },
  };
}

/** Kế hoạch ăn và lịch tập mẫu: điền sẵn cho form tạo lộ trình và là nội dung tab Kế hoạch ở chi tiết lộ trình của PT. */
// TODO(ROUTE): form bắt đầu trống hoặc nạp bản nháp của PT (adr.md, N22); tab Kế hoạch đọc từ API (muc_ke_hoach, K10).
export function taoKeHoachMau(): KeHoachLoTrinh {
  return {
    buaAn: [
      {
        id: 'sang',
        ten: 'Bữa sáng',
        gio: '07:00',
        bat: true,
        moTa: 'Yến mạch với sữa, 3 trứng luộc, 1 chuối',
        kcal: 550,
      },
      {
        id: 'trua',
        ten: 'Bữa trưa',
        gio: '12:00',
        bat: true,
        moTa: 'Cơm gạo lứt, ức gà áp chảo, rau xanh luộc',
        kcal: 700,
      },
      {
        id: 'toi',
        ten: 'Bữa tối',
        gio: '19:00',
        bat: true,
        moTa: 'Khoai lang, cá hồi nướng, salad',
        kcal: 600,
      },
    ],
    lichTap: {
      chung: [
        {
          thu: 1,
          ten: 'Upper A',
          gio: '17:30',
          nhomCo: 'Ngực, vai, tay sau',
          thoiLuongPhut: 60,
          baiTap: [
            { id: 'ua-1', ten: 'Đẩy ngực với tạ đòn', soSet: 4, soLan: '8–10 rep' },
            { id: 'ua-2', ten: 'Đẩy vai với tạ đơn', soSet: 3, soLan: '10 rep' },
            { id: 'ua-3', ten: 'Đẩy tay sau với dây cáp', soSet: 3, soLan: '12 rep' },
          ],
        },
        {
          thu: 3,
          ten: 'Lower A',
          gio: '17:30',
          nhomCo: 'Chân, mông',
          thoiLuongPhut: 60,
          baiTap: [
            { id: 'la-1', ten: 'Squat với tạ đòn', soSet: 4, soLan: '8 rep' },
            { id: 'la-2', ten: 'Romanian deadlift', soSet: 3, soLan: '10 rep' },
            { id: 'la-3', ten: 'Đạp đùi trên máy', soSet: 3, soLan: '12 rep' },
          ],
        },
        {
          thu: 5,
          ten: 'Upper B',
          gio: '17:30',
          nhomCo: 'Lưng, tay trước',
          thoiLuongPhut: 60,
          baiTap: [
            { id: 'ub-1', ten: 'Kéo xà', soSet: 4, soLan: '6–8 rep' },
            { id: 'ub-2', ten: 'Kéo cáp ngồi', soSet: 3, soLan: '10 rep' },
            { id: 'ub-3', ten: 'Cuốn tạ đơn', soSet: 3, soLan: '12 rep' },
          ],
        },
      ],
      rieng: {},
    },
  };
}

const MUC_TIEU_QUAN_LY: Record<string, { tu: number; den: number; donVi: string }> = {
  'ql-2': { tu: 70, den: 64, donVi: 'kg' },
  'ql-3': { tu: 72, den: 68, donVi: 'kg' },
};
const MUC_TIEU_MAC_DINH = { tu: 60, den: 70, donVi: 'kg' };

/** Lượt tham gia mẫu: lộ trình nhóm đang chạy có một người mới được mời, chưa chấp nhận (N23). */
function taoThanhVienMau(lo: LoTrinhQuanLy): ThanhVienLoTrinh[] {
  const coNguoiChoChapNhan = lo.loai === 'nhom' && lo.trangThai === 'dang_hoat_dong';
  return lo.nguoiThamGia.map((n, i) => ({
    ...n,
    thamGia: coNguoiChoChapNhan && i === lo.nguoiThamGia.length - 1 ? 'cho_chap_nhan' : 'tham_gia',
  }));
}

/** Nhiệm vụ hôm nay chung của lộ trình; nhóm thì một vài thành viên đã check-in nhiệm vụ chưa xong. */
function taoNhiemVuNhomMau(thanhVien: readonly ThanhVienLoTrinh[]): NhiemVuNhom[] {
  const dangThamGia = thanhVien.filter((t) => t.thamGia === 'tham_gia').map((t) => t.id);
  return [
    {
      id: 'nvn-1',
      loai: 'bua_an',
      tieuDe: 'Bữa sáng',
      moTa: 'Chờ học viên check-in',
      gio: '07:00',
      moc: 'cho_check_in',
      daCheckIn: dangThamGia,
    },
    {
      id: 'nvn-2',
      loai: 'bua_an',
      tieuDe: 'Bữa trưa',
      moTa: 'Chờ học viên check-in',
      gio: '12:00',
      moc: 'cho_check_in',
      daCheckIn: dangThamGia.slice(0, dangThamGia.length > 1 ? 2 : 0),
    },
    {
      id: 'nvn-3',
      loai: 'buoi_tap',
      tieuDe: 'Buổi tập',
      moTa: 'Upper A',
      gio: '17:30',
      moc: 'sap_toi',
      daCheckIn: [],
    },
    {
      id: 'nvn-4',
      loai: 'can_nang',
      tieuDe: 'Cân nặng',
      moTa: 'Cập nhật định kỳ',
      gio: '07:00',
      moc: 'ngay_mai',
      daCheckIn: [],
    },
  ];
}

/** Check-in mẫu: số bản chờ duyệt khớp \`soCheckInChoDuyet\` của thẻ, chia đều cho các thành viên, kèm vài bản đã xử lý. */
function taoCheckInQuanLyMau(
  lo: LoTrinhQuanLy,
  thanhVien: readonly ThanhVienLoTrinh[],
): CheckInQuanLy[] {
  const dangThamGia = thanhVien.filter((t) => t.thamGia === 'tham_gia');
  const dau = dangThamGia[0];
  if (!dau) return [];
  const choDuyet: CheckInQuanLy[] = Array.from({ length: lo.soCheckInChoDuyet }, (_, i) => ({
    id: `ci-${lo.id}-${i + 1}`,
    loai: i % 2 === 0 ? 'bua_an' : 'buoi_tap',
    tieuDe: i % 2 === 0 ? 'Bữa trưa' : 'Buổi tập Upper A',
    thoiDiem: i % 2 === 0 ? 'Hôm nay, 12:00' : 'Hôm nay, 06:30',
    hocVien: dangThamGia[i % dangThamGia.length] ?? dau,
    trangThai: 'cho_duyet',
  }));
  return [
    ...choDuyet,
    {
      id: `ci-${lo.id}-da-1`,
      loai: 'bua_an',
      tieuDe: 'Bữa sáng',
      thoiDiem: 'Hôm nay, 07:00',
      hocVien: dau,
      trangThai: 'da_duyet',
    },
    {
      id: `ci-${lo.id}-da-2`,
      loai: 'buoi_tap',
      tieuDe: 'Buổi tập Cardio & Core',
      thoiDiem: 'Hôm qua, 06:00',
      hocVien: dau,
      trangThai: 'can_bo_sung',
    },
  ];
}

/** Chi tiết lộ trình của PT (PT-03) dựng từ thẻ ở PT-01, dành cho cả lộ trình đơn lẫn nhóm. */
// TODO(ROUTE): thay bằng API chi tiết lộ trình của PT (thành viên, nhiệm vụ hôm nay, check-in, hoạt động gần đây, kế hoạch).
export function taoChiTietQuanLyMau(lo: LoTrinhQuanLy, homNay: Date): ChiTietQuanLy {
  const thanhVien = taoThanhVienMau(lo);
  const dangChay = lo.trangThai === 'dang_hoat_dong';
  const dau = thanhVien[0];
  return {
    loTrinh: lo,
    pt: ptTranDucAnh,
    tuanHienTai: tuanHienTaiCua(lo.ngayBatDau, homNay, lo.tongTuan),
    mucTieu: MUC_TIEU_QUAN_LY[lo.id] ?? MUC_TIEU_MAC_DINH,
    thanhVien,
    nhiemVuHomNay: dangChay ? taoNhiemVuNhomMau(thanhVien) : [],
    checkIn: taoCheckInQuanLyMau(lo, thanhVien),
    hoatDong:
      dangChay && dau
        ? [
            {
              id: 'hd-1',
              nguoi: ptTranDucAnh,
              noiDung: `PT vừa phản hồi bữa sáng của ${dau.ten}`,
              thoiDiem: 'Vừa xong',
            },
            {
              id: 'hd-2',
              nguoi: dau,
              noiDung: `${dau.ten} đã check-in bữa sáng`,
              thoiDiem: '07:15',
            },
          ]
        : [],
    keHoach: taoKeHoachMau(),
  };
}
