"""Sinh SQL seed khu vực hai cấp (tỉnh/thành, phường/xã) cho bảng danh_muc từ file Excel.

Chỉ dùng thư viện chuẩn. Chạy từ gốc repo:

    python database/scripts/tao-seed-khu-vuc.py <file.xlsx>

Cột dùng ở sheet đầu: C = mã tỉnh (BNV), D = tên tỉnh/TP, I = mã phường/xã (TMS), J = tên phường/xã.
Dữ liệu sai (thiếu, sai dạng, trùng mã) thì dừng, không sinh file.
"""

import pathlib
import re
import sys
import unicodedata
import uuid
import xml.etree.ElementTree as ET
import zipfile

NS = "{http://schemas.openxmlformats.org/spreadsheetml/2006/main}"
REL = "{http://schemas.openxmlformats.org/officeDocument/2006/relationships}"
DICH = pathlib.Path(__file__).resolve().parents[2] / (
    "apps/api/src/Data/GymBase.Persistence/Migrations/Sql/SeedKhuVuc.sql"
)
KHONG_GIAN_ID = uuid.uuid5(uuid.NAMESPACE_URL, "gymbase/danh_muc")
# Tên tỉnh/thành bỏ tiền tố hành chính để gọn trong dropdown.
TIEN_TO = ("Thành phố ", "Tỉnh ", "Tp ")
TEN_RIENG = {"Hồ Chí Minh": "TP. Hồ Chí Minh"}
SO_DONG_MOI_LENH = 500


def doc_sheet_dau(duong_dan):
    """Các dòng của sheet đầu tiên, mỗi dòng là {chữ cột: giá trị dạng chữ}."""
    with zipfile.ZipFile(duong_dan) as z:
        chuoi = []
        if "xl/sharedStrings.xml" in z.namelist():
            for si in ET.fromstring(z.read("xl/sharedStrings.xml")).findall(NS + "si"):
                chuoi.append("".join(t.text or "" for t in si.iter(NS + "t")))
        lien_ket = {r.get("Id"): r.get("Target") for r in ET.fromstring(z.read("xl/_rels/workbook.xml.rels"))}
        sheet = next(ET.fromstring(z.read("xl/workbook.xml")).iter(NS + "sheet"))
        dich = lien_ket[sheet.get(REL + "id")].lstrip("/")
        dich = dich if dich.startswith("xl/") else "xl/" + dich
        for row in ET.fromstring(z.read(dich)).iter(NS + "row"):
            o = {}
            for c in row.findall(NS + "c"):
                v = c.find(NS + "v")
                if c.get("t") == "inlineStr":
                    gia_tri = "".join(t.text or "" for t in c.iter(NS + "t"))
                elif v is None:
                    gia_tri = ""
                else:
                    gia_tri = chuoi[int(v.text)] if c.get("t") == "s" else v.text
                o[re.match("[A-Z]+", c.get("r")).group(0)] = gia_tri
            yield o


def chuan_hoa(chu):
    return " ".join(unicodedata.normalize("NFC", chu).split())


def ten_tinh(chu):
    for tien_to in TIEN_TO:
        if chu.startswith(tien_to):
            chu = chu[len(tien_to):]
            break
    return TEN_RIENG.get(chu, chu)


def doc_du_lieu(duong_dan):
    tinh, xa, da_gap_tieu_de = {}, {}, False
    for so, o in enumerate(doc_sheet_dau(duong_dan), start=1):
        ma_tinh, ten, ma_xa, ten_xa = (chuan_hoa(o.get(cot, "")) for cot in "CDIJ")
        if not da_gap_tieu_de:
            da_gap_tieu_de = ma_tinh.startswith("Mã tỉnh")
            continue
        if not (re.fullmatch(r"\d{2}", ma_tinh) and re.fullmatch(r"\d{8}", ma_xa) and ten and ten_xa):
            sys.exit(f"Dòng dữ liệu thứ {so} thiếu hoặc sai dạng: {o}")
        if "/" in ten_xa:
            sys.exit(f"Tên có dấu '/', làm hỏng muc_luc: {ten_xa}")
        ten = ten_tinh(ten)
        if tinh.setdefault(ma_tinh, ten) != ten:
            sys.exit(f"Mã tỉnh {ma_tinh} có hai tên: {tinh[ma_tinh]} và {ten}")
        if ma_xa in xa:
            sys.exit(f"Trùng mã phường/xã {ma_xa}")
        xa[ma_xa] = (ma_tinh, ten_xa)
    if not xa:
        sys.exit("Không đọc được dòng nào; kiểm tra dòng tiêu đề và cột C, D, I, J.")
    if len(set(tinh.values())) != len(tinh):
        sys.exit("Hai mã tỉnh trùng tên.")
    return tinh, xa


def sql_chuoi(chu):
    return "NULL" if chu is None else "'" + chu.replace("\\", "\\\\").replace("'", "''") + "'"


def dong(ma, ten, cap, cha, muc_luc, muc_luc_ma):
    ma_id = str(uuid.uuid5(KHONG_GIAN_ID, "khu_vuc/" + ma))
    cha_id = None if cha is None else str(uuid.uuid5(KHONG_GIAN_ID, "khu_vuc/" + cha))
    gia_tri = [sql_chuoi(x) for x in (ma_id, "khu_vuc", ma, ten)]
    gia_tri += [str(cap), sql_chuoi(cha_id), sql_chuoi(muc_luc), sql_chuoi(muc_luc_ma), "1", "UTC_TIMESTAMP(6)"]
    return "(" + ", ".join(gia_tri) + ")"


def lenh_insert(cac_dong):
    for i in range(0, len(cac_dong), SO_DONG_MOI_LENH):
        yield (
            "INSERT INTO `danh_muc` (`Id`, `ma_dinh_danh`, `ma`, `ten`, `cap`, `danh_muc_cap_tren_id`, "
            "`muc_luc`, `muc_luc_ma`, `tinh_trang`, `ngay_tao`) VALUES\n"
            + ",\n".join(cac_dong[i:i + SO_DONG_MOI_LENH])
            + ";\n"
        )


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    tinh, xa = doc_du_lieu(sys.argv[1])
    cap_1 = [
        dong(f"tinh_{ma}", ten, 1, None, ten, f"khu_vuc/tinh_{ma}")
        for ma, ten in sorted(tinh.items())
    ]
    cap_2 = [
        dong(f"px_{ma}", ten, 2, f"tinh_{ma_tinh}", f"{tinh[ma_tinh]}/{ten}", f"khu_vuc/tinh_{ma_tinh}/px_{ma}")
        for ma, (ma_tinh, ten) in sorted(xa.items(), key=lambda x: (x[1][0], x[0]))
    ]
    dau = (
        "-- File sinh tự động bởi database/scripts/tao-seed-khu-vuc.py, không sửa tay.\n"
        f"-- {len(cap_1)} tỉnh/thành, {len(cap_2)} phường/xã.\n"
    )
    DICH.parent.mkdir(parents=True, exist_ok=True)
    # Tỉnh chèn trước phường/xã vì khóa ngoại tự tham chiếu được kiểm tra ngay.
    DICH.write_text(dau + "".join(lenh_insert(cap_1)) + "".join(lenh_insert(cap_2)), encoding="utf-8", newline="\n")
    print(f"Đã ghi {len(cap_1)} tỉnh/thành, {len(cap_2)} phường/xã vào {DICH}")


if __name__ == "__main__":
    main()
