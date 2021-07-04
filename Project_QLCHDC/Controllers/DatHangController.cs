using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_QLCHDC.Models;

namespace Project_QLCHDC.Controllers
{
    public class DatHangController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext();
        //
        // GET: /DatHang/
        public ActionResult ThemMatHang(string masp, FormCollection c)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            else
            {
                SANPHAM sptt = db.SANPHAMs.Where(sp => sp.MASP == masp).Single();
                int gia;
                
                    gia = sptt.GIABAN ?? default(int);
                
                int sl = int.Parse(c["txtQuantity"]);
                TAIKHOAN k = Session["kh"] as TAIKHOAN;
                List<CART_ITEM> gh = db.CART_ITEMs.Where(s => s.TENTK == k.TENTK).ToList();
                ShopCart shop = new ShopCart();
                if (gh != null)
                {
                    shop.list = gh;
                }
                shop.Them(masp, k.TENTK, sptt.TENSP, gia, sptt.HINHANH, sl);
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
        }

        public ActionResult GioHang()
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN kh = Session["kh"] as TAIKHOAN;
            List<CART_ITEM> listCart = db.CART_ITEMs.Where(s => s.TENTK == kh.TENTK && s.TRANGTHAI == 0).ToList();
            ShopCart gh = new ShopCart(listCart);
            bool hasCart = db.CART_ITEMs.Where(k => k.TENTK == kh.TENTK).Count() > 0;
            if (!hasCart)
            {
                ViewBag.cart = "Giỏ Hàng Rỗng !";
            }
            return View(gh);
        }
        [HttpPost]
        public ActionResult EditQuantity(FormCollection c)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            else
            {
                TAIKHOAN kh = Session["kh"] as TAIKHOAN;
                string[] quantities = c.GetValues("txtQuantity");
                List<CART_ITEM> listitem = db.CART_ITEMs.Where(s => s.TENTK == kh.TENTK && s.TRANGTHAI == 0).ToList();
                for (int i = 0; i < listitem.Count(); i++)
                {
                    listitem[i].SOLUONG = Convert.ToInt32(quantities[i]);
                    listitem[i].THANHTIEN = long.Parse((listitem[i].SOLUONG * listitem[i].GIABAN).ToString());
                    db.SubmitChanges();
                }
                ShopCart shop = new ShopCart(listitem);
                Session["gh"] = shop;
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
        }

        //

        public ActionResult ClearCart(bool confirm)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN kh = Session["kh"] as TAIKHOAN;
            if (confirm)
            {
                List<CART_ITEM> listCart = db.CART_ITEMs.Where(s => s.TENTK == kh.TENTK && s.TRANGTHAI == 0).ToList();
                foreach (CART_ITEM c in listCart)
                {
                    db.CART_ITEMs.DeleteOnSubmit(c);

                }
                db.SubmitChanges();
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        //
        public ActionResult DeleteCartItem(string masp, string kt)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN kh = Session["kh"] as TAIKHOAN;
            CART_ITEM c = db.CART_ITEMs.Where(s => s.MASP == masp && s.TENTK == kh.TENTK).Single();
            db.CART_ITEMs.DeleteOnSubmit(c);
            db.SubmitChanges();
            return RedirectToAction("GioHang", "DatHang");
        }

        //
        [HttpGet]
        public ActionResult ThanhToan()
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN kh = Session["kh"] as TAIKHOAN;
            List<CART_ITEM> list = db.CART_ITEMs.Where(s => s.TENTK == kh.TENTK && s.TRANGTHAI == 0).ToList();
            ShopCart sh = new ShopCart(list);
            return View(sh);
        }

        [HttpPost]
        public ActionResult ThanhToan(FormCollection c)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN kh = Session["kh"] as TAIKHOAN;
            bool matchPass = db.TAIKHOANs.Where(k => k.TENTK == kh.TENTK && k.MATKHAU.Trim() == c["txtPassword"].ToString().Trim()).Count() > 0;
            if (matchPass == false)
            {
                Session["wrong"] = "Sai mật khẩu !";
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            string address = c["txtDiaChi"].ToString().Trim()+"," + c["txtQuan"].ToString().Trim()+"," + c["txtThanhPho"].ToString().Trim();
            List<CART_ITEM> list = db.CART_ITEMs.Where(sp => sp.TENTK == kh.TENTK && sp.TRANGTHAI == 0).ToList();
            ShopCart s = new ShopCart(list);
            HOADON hd = new HOADON();
            hd.NGAYDAT = DateTime.Now;
            hd.TENTK = kh.TENTK;
            hd.DIACHIGIAO = address;
            hd.TRANGTHAI = "Chưa giao";
            hd.TONGTIEN = s.TongThanhTien();
            db.HOADONs.InsertOnSubmit(hd);
            db.SubmitChanges();
            foreach (CART_ITEM item in s.list)
            {
                CHITIETHOADON ct = new CHITIETHOADON();
                SANPHAM sanpham = new SANPHAM();
                ct.MAHD = hd.MAHD;
                ct.MASP = item.MASP;
                ct.SOLUONG = item.SOLUONG;
                ct.THANHTIEN = item.SOLUONG * item.GIABAN;
                ct.GIABAN = item.GIABAN;
                sanpham = db.SANPHAMs.Where(sp => sp.MASP == item.MASP).FirstOrDefault();
                sanpham.SOLUONGTON = sanpham.SOLUONGTON - item.SOLUONG;
                db.CHITIETHOADONs.InsertOnSubmit(ct);
                db.SubmitChanges();
            }
            foreach (CART_ITEM item in list)
            {
                item.TRANGTHAI = 1;
                db.SubmitChanges();
            }
            List<CART_ITEM> datt = db.CART_ITEMs.Where(tt => tt.TENTK == kh.TENTK && tt.TRANGTHAI == 1).ToList();
            foreach (CART_ITEM cit in datt)
            {
                db.CART_ITEMs.DeleteOnSubmit(cit);
                db.SubmitChanges();
            }
            db.SubmitChanges();
            Session["hd"] = "Đặt hàng thành công, quý khách vui lòng kiểm tra lại đơn hàng trong lịch sử giao dịch, xin cám ơn!";
            return RedirectToAction("Index", "KhachHang");
        }
    }
}