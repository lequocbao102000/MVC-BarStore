using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using Project_QLCHDC.Models;

namespace Project_QLCHDC.Controllers
{
    public class ProductController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext();
        //
        // GET: /Product/
        public ActionResult Category(string sale, string searchBy, string madm, int? page)
        {
            List<DANHMUC> ll = db.DANHMUCs.ToList();
            ViewBag.loai = ll;
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
            
            if (madm != null)
            {
                return View(db.SANPHAMs.Where(s => s.MADM == madm).ToList().ToPagedList(page ?? 1, 12));
            }
            else if (madm != null)
            {
                return View(db.SANPHAMs.Where(s => s.MADM == madm).ToList().ToPagedList(page ?? 1, 12));
            }
            else
            {
                return View(db.SANPHAMs.ToList().ToPagedList(page ?? 1, 12));
            }
        }
        public ActionResult CategoriesOp()
        {
            return PartialView();
        }
        public ActionResult ProductDetail(string masp)
        {
            if (masp == null)
            {
                return RedirectToAction("Index", "KhachHang");
            }
            SANPHAM s = db.SANPHAMs.Where(sp => sp.MASP == masp).Single();
            if (Session["kh"] != null)
            {
                TAIKHOAN k = Session["kh"] as TAIKHOAN;
                List<SANPHAM> thich = new List<SANPHAM>();
                var data = from sp in db.SANPHAMs
                           join th in db.SPTHICHes
                           on sp.MASP equals th.MASP
                           where th.TENTK == k.TENTK
                           select sp;
                foreach (var item in data)
                {
                    SANPHAM ss = item as SANPHAM;
                    thich.Add(ss);
                }
                ViewBag.thich = thich;
            }
            List<SANPHAM> related = db.SANPHAMs.Where(sp => sp.MADM == s.MADM && sp.MASP != s.MASP).Take(4).ToList();
            ViewBag.related = related;
           
            return View(s);
        }
        public ActionResult AddLoveProduct(string masp)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            TAIKHOAN k = Session["kh"] as TAIKHOAN;
            SPTHICH s = new SPTHICH();
            bool check = db.SPTHICHes.Where(sp => sp.MASP == masp && sp.TENTK == k.TENTK).Count() > 0;
            if (check)
            {
                SPTHICH sp = db.SPTHICHes.Where(ss => ss.MASP == masp && ss.TENTK == k.TENTK).Single();
                db.SPTHICHes.DeleteOnSubmit(sp);
                db.SubmitChanges();
                return Redirect(Request.UrlReferrer.ToString());
            }
            else
            {
                s.MASP = masp;
                s.TENTK = k.TENTK;
                db.SPTHICHes.InsertOnSubmit(s);
                db.SubmitChanges();
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
        }
    }
}