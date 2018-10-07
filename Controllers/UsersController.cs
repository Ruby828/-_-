using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication2.Controllers
{
    public class UsersController : Controller
    {
        private Database1Entities db = new Database1Entities();

        public static string Encrypt(string plainText)
        {
            byte[] data = Encoding.Default.GetBytes(plainText);
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] result = sha256.ComputeHash(data);
            return Convert.ToBase64String(result);

        }
        public ActionResult Login(InputForm form)
        {
            ViewBag.username = form.FormUsername;
            ViewBag.password = form.FormPassword;
            var r = (from a in db.Users
                    where a.UserName == form.FormUsername
                    select a).FirstOrDefault();
            if (r == null)
            {
                ViewBag.message = "帳號密碼錯誤";
                return View();
            } 
            else
            {
                string SaltAndFormPassword = String.Concat(r.Id, form.FormPassword);
                string FormPassword = Encrypt(SaltAndFormPassword);
                ViewBag.inputHPV = FormPassword;
                ViewBag.savedHPV = r.PassWord;
                if (string.Compare(FormPassword, r.PassWord, false) == 0) ViewBag.message = "登入成功";
                else ViewBag.message = "帳號密碼錯誤";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,username,password")] Table table)
        {
            if (ModelState.IsValid)
            {
                table.Id = Guid.NewGuid();
                string SaltAndPassword = String.Concat(table.Id, table.password);
                string hashedPV = Encrypt(SaltAndPassword);
                table.password = hashedPV;
                db.Tables.Add(table);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        // GET: Users
        [Authorize]
        public ActionResult Test()
        {
            return View();
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return View();
        }

        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserName,PassWord")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,PassWord")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
