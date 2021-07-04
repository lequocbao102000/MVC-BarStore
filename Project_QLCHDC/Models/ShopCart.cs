using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Project_QLCHDC.Models;

namespace Project_QLCHDC.Models
{
    public class ShopCart
    {
        DataClasses1DataContext db = new DataClasses1DataContext();
        public List<CART_ITEM> list;
        public ShopCart()
        {
            list = new List<CART_ITEM>();
        }
        public ShopCart(List<CART_ITEM> listGh)
        {
            list = listGh;
        }
        public int SoMatHang()
        {
            if (list == null)
                return 0;
            return list.Count();
        }

        public int? TongSLHang()
        {
            if (list == null)
                return 0;
            return list.Sum(s => s.SOLUONG);
        }

        public long? TongThanhTien()
        {
            if (list == null)
                return 0;
            return list.Sum(s => s.THANHTIEN);
        }

        public int Them(string masp, string username, string tensp, int gia, string hinh, int sl)
        {
            CART_ITEM sp = list.SingleOrDefault(s => s.MASP == masp );
            bool hasitem = db.CART_ITEMs.Where(s => s.MASP == masp && s.TENTK == username).Count() > 0;
            if (sp == null)
            {
                CART_ITEM item = new CART_ITEM();
                item.MASP = masp;
                item.TENTK = username;
                item.SOLUONG = sl;
                item.TRANGTHAI = 0;
                item.GIABAN = gia;
                item.TENSP = tensp;
                item.HINH = hinh;
                item.THANHTIEN = long.Parse((item.SOLUONG * item.GIABAN).ToString());
                if (item == null)
                    return -1;            
                list.Add(item);

                db.CART_ITEMs.InsertOnSubmit(item);
                db.SubmitChanges();
            }
            else
            {
                sp.SOLUONG++;
                sp.THANHTIEN = long.Parse((sp.SOLUONG * sp.GIABAN).ToString());
                CART_ITEM itemhas = db.CART_ITEMs.Where(s => s.MASP == masp && username == s.TENTK).Single();
                itemhas.SOLUONG++;
                itemhas.THANHTIEN = long.Parse((itemhas.SOLUONG * itemhas.GIABAN).ToString());
                db.SubmitChanges();
            }
            return 1;
        }
    }
}