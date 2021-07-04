using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project_QLCHDC.Models;

namespace Project_QLCHDC.Controllers
{
    public class KhachHangController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext();
        //
        // GET: /KhachHang/
        public ActionResult Index()
        {
            //Lay loai san pham
            List<DANHMUC> listLoai = db.DANHMUCs.ToList();
            ViewBag.categories = listLoai;

            //Lay cac san pham trend
            var trend = db.SPTHICHes.Select(s => s.MASP).Distinct().Take(3).ToList();
            ViewBag.trend = trend;

            //Lay sp thich cua user
            if (Session["kh"] != null)
            {
                TAIKHOAN k = Session["kh"] as TAIKHOAN;
                List<SANPHAM> listSpThich = (from s in db.SANPHAMs
                                             join t in db.SPTHICHes
                                             on s.MASP equals t.MASP
                                             where t.TENTK == k.TENTK
                                             select s).ToList();
                ViewBag.thich = listSpThich;
            }

            //Lay san pham
            List<SANPHAM> listSp = db.SANPHAMs.Take(10).ToList();

            //lAY Gio Hang chua thanh toan

            if (Session["kh"] != null)
            {
                TAIKHOAN kh = Session["kh"] as TAIKHOAN;
                List<CART_ITEM> cart = db.CART_ITEMs.Where(s => s.TENTK == kh.TENTK && s.TRANGTHAI == 0).ToList();
                ShopCart shop = new ShopCart(cart);
                Session["gh"] = shop;
            }
            return View(listSp);
        }
        //

        [HttpGet]
        public ActionResult Signin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Signin(FormCollection c)
        {
            if (Session["kh"] != null)
            {
                return RedirectToAction("Index", "KhachHang");
            }
            TAIKHOAN kh = db.TAIKHOANs.FirstOrDefault(k => k.TENTK == c["txtUsername"] && k.MATKHAU == c["txtPassword"]
                                                        &&k.VAITRO=="Khách hàng"&&k.TRANGTHAI=="ok");
            if (kh != null)
            {
                Session["kh"] = kh;
                return RedirectToAction("Index", "KhachHang");
            }
            else
            {
                Session["tb"] = "Tài khoản hoặc mật khẩu không đúng !";
                return RedirectToAction("Signin", "KhachHang");
            }
        }
        //
        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Signup(FormCollection c)
        {
            bool check = db.TAIKHOANs.Where(k => k.TENTK == c["txtUsername"]).Count() > 0;
            if (check)
            {
                ViewBag.hasUser = "Tài khoản này đã tồn tại";
                return View();
            }
            else
            {
                TAIKHOAN k = new TAIKHOAN();
                k.TENTK = c["txtUsername"];
                k.HOTEN = c["txtHo"] + " " + c["txtTen"];
                k.VAITRO = "Khách hàng";
                k.TRANGTHAI = "ok";
                k.SDT = c["txtPhone"];
                k.EMAIL = c["txtEmail"];
                k.DIACHI = c["txtAddress"];
                k.QUEQUAN = c["txtQueQuan"];
                k.THANHPHO = c["txtCity"];
                k.MATKHAU = c["txtPassword"];
                string url = "default-avatar.jpg";
                k.HINH = url;
                db.TAIKHOANs.InsertOnSubmit(k);
                db.SubmitChanges();
                Session["tb"] = "Đăng ký tài khoản thành công !";
                Session["kh"] = k;
                return RedirectToAction("Index", "KhachHang");
            }
        }
        public ActionResult KhachHangNow()
        {
            TAIKHOAN k = Session["kh"] as TAIKHOAN;
            //List<SPTHICH> listSpThich = db.SPTHICHes.Where(s => s.USERNAME == k.USERNAME).ToList();
            List<SPTHICH> thich = new List<SPTHICH>();

            if (k != null)
            {
                thich = db.SPTHICHes.Where(s => s.TENTK == k.TENTK).ToList();
                List<CART_ITEM> gh = db.CART_ITEMs.Where(s => s.TENTK == k.TENTK && s.TRANGTHAI == 0).ToList();
                ShopCart sh = new ShopCart(gh);
                ViewBag.hang = sh;
            }

            ViewBag.thich = thich;
            return PartialView(k);
        }

        //Log out
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "KhachHang");
        }

        public ActionResult SanPhamThich()
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN k = Session["kh"] as TAIKHOAN;
            List<SANPHAM> thich = new List<SANPHAM>();
            List<string> listmspthich = new List<string>();
            List<string> listmsprelated = new List<string>();
            var data = from s in db.SANPHAMs
                       join t in db.SPTHICHes
                       on s.MASP equals t.MASP
                       where t.TENTK == k.TENTK
                       select s;
            foreach (var item in data)
            {
                SANPHAM s = item as SANPHAM;
                thich.Add(s);
            }
            foreach (SANPHAM sq in thich)
            {
                listmspthich.Add(sq.MASP);
            }
            //Related
            List<SANPHAM> related = db.SANPHAMs.ToList();
            int randomRecord = new Random().Next() % related.Count(); //To make sure its valid index in list
            List<SANPHAM> qData = related.Skip(randomRecord).Take(8).ToList();
            foreach (SANPHAM qs in qData)
            {
                listmsprelated.Add(qs.MASP);
            }
            IEnumerable<string> relatedSp = listmsprelated.Except(listmspthich).ToList();

            List<SANPHAM> asd = new List<SANPHAM>();
            foreach (string sq in relatedSp)
            {
                SANPHAM s = db.SANPHAMs.Where(sw => sw.MASP == sq).Single();
                asd.Add(s);
            }
            ViewBag.related = asd;
            return View(thich);
        }

        //
        public ActionResult ThongTin(string username)
        {

            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            else
            {
                TAIKHOAN k = Session["kh"] as TAIKHOAN;
                List<CHITIETHOADON> data = (from c in db.CHITIETHOADONs
                                            join d in db.HOADONs
                                            on c.MAHD equals d.MAHD
                                            where d.TENTK == k.TENTK
                                            select c).ToList();
                List<HOADON> datahd = db.HOADONs.Where(c => c.TENTK == k.TENTK).ToList();
                ViewBag.cthd = data;
                ViewBag.lhd = datahd;
                return View(k);
            }
        }
    }
}