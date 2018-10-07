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
    public class TablesController : Controller
    {
        private Database1Entities1 db = new Database1Entities1();

        public ActionResult Member() //會員專區
        {
            return View();
        }

        public static string Encrypt(string plainText)
        {
            byte[] data = Encoding.Default.GetBytes(plainText);
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] result = sha256.ComputeHash(data);
            return Convert.ToBase64String(result);
        }
        public ActionResult Login(InputForm form)
        {
            ViewBag.account = form.FormAccount;
            ViewBag.password = form.FormPassword;
            var r = (from a in db.User
                     where a.Account == form.FormAccount
                     select a).FirstOrDefault();
            if (r == null)
            {
                ViewBag.message = "請輸入帳號密碼";
                return View();
            }
            else
            {
                string SaltAndFormPassword = String.Concat(r.Uid, form.FormPassword);
                string FormPassword = Encrypt(SaltAndFormPassword);
                ViewBag.inputHPW = FormPassword;
                ViewBag.savedHPW = r.Password;
                if (string.Compare(FormPassword, r.Password, false) == 0)
                {
                    ViewBag.message = "登入成功";
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                   r.Account, DateTime.Now, DateTime.Now.AddMinutes(30),
                   false,r.Uid.ToString(), FormsAuthentication.FormsCookiePath);
                    string encTicket = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                    cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("Test", "Tables");   //登入成功回首頁
                }
                else
                {
                    ViewBag.message = "帳號密碼錯誤，請再輸入一次";
                    return View();
                } 
               
                
            }
            
        }
        //修改密碼
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [Authorize]
        public ActionResult ModPassword(ModPassword form, string Account)
        {    
            User user = (from a in db.User
                         where a.Account == Account
                     select a).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            else   //如果讀的到->將新密碼連接Uid做hash 和Uid加上舊的密碼做hash後的結果比對
            {
                ////輸入的舊密碼做hash
                string SaltAndFormPasswordOld = String.Concat(user.Uid, form.FormOldPassword);
                string FormPasswordOld = Encrypt(SaltAndFormPasswordOld);

                if (string.Compare(FormPasswordOld, user.Password, false) == 0)  //表示使用者有輸入正確的舊密碼
                {
                    ViewBag.message = "舊密碼正確";
                    if (string.Compare(form.FormNewPassword, form.FormCheckPassword, false) == 0)  //"新密碼"、"確認密碼" 相同
                    {
                        //輸入的新密碼做hash
                        string SaltAndFormPasswordNew = String.Concat(user.Uid, form.FormNewPassword);
                        string FormPasswordNew = Encrypt(SaltAndFormPasswordNew);

                        user.Password = FormPasswordNew;
                        db.SaveChanges();
                        //設定成功訊息
                        TempData["ResultMessage"] = String.Format("使用者[{0}]成功編輯", user.Uname);
                        return RedirectToAction("Login", "Tables");       //成功改好密碼 回到會員資料
                    }
                }
                else
                {
                    ViewBag.message = "舊密碼驗證錯誤";
                    return View();
                }
                return RedirectToAction("Edit", "Tables");   //修改失敗
             }     
         }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Authorize]
        public ActionResult Test()
        {
            string sUserID = User.Identity.Name;
            return View();
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return View();
        }
        // GET: Tables

        public ActionResult Index()
        {
            return View(db.User.ToList());
        }
        /// ////////////////////////////////////////////DETAILS//////////////////////////////////////////////////
        // GET: Tables/Details/5
        [Authorize]
        public ActionResult Details(string Account)   //顯示個人資料
        {
            FormsAuthentication.SetAuthCookie(Account, false);
            HttpCookie cookie = this.Request.Cookies[FormsAuthentication.FormsCookieName];
            //get authen coolie
            FormsAuthenticationTicket oldTicket = FormsAuthentication.Decrypt(cookie.Value);
            if (oldTicket.Name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            @ViewBag.message = oldTicket.Name;  //Account
            User user = (from a in db.User
                         where a.Account == oldTicket.Name
                         select a).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //////////////////////CREATE///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GET: Tables/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tables/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "Uid")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Uid = Guid.NewGuid();
                string SaltAndPassword = String.Concat(user.Uid, user.Password);
                string hashedPW = Encrypt(SaltAndPassword);
                user.Password = hashedPW;
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }
        /////////////////EDIT///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GET: Tables/Edit/5
        [Authorize]
        public ActionResult Edit(string Account)
        {
            FormsAuthentication.SetAuthCookie(Account, false);
            HttpCookie cookie = this.Request.Cookies[FormsAuthentication.FormsCookieName];
            //get authen coolie
            FormsAuthenticationTicket oldTicket = FormsAuthentication.Decrypt(cookie.Value);
            if (oldTicket.Name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            @ViewBag.message = oldTicket.Name;  //Account
            User user = (from a in db.User
                         where a.Account == oldTicket.Name
                         select a).FirstOrDefault();
            //@ViewBag.message1 = user.Account;  //Account
            //@ViewBag.message2 = user.Password;  //Account
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Tables/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]  //用於更新
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit(Models.User postback)
        {
            using (Models.Database1Entities1 db = new Models.Database1Entities1())
            {
                var result = (from s in db.User                         
                              where s.Uid == postback.Uid
                              select s).FirstOrDefault();
                if (result != default(Models.User))
                {
                    result.Uname = postback.Uname;
                    result.Phone = postback.Phone;
                    result.Address = postback.Address;
                    db.SaveChanges();
                    //設定成功訊息
                    TempData["ResultMessage"] = String.Format("使用者[{0}]成功編輯", postback.Uname);
                    return RedirectToAction("Details","Tables");
                }
            }
            //設定錯誤訊息
            TempData["ResultMessage"] = String.Format("使用者[{0}]不存在，請重新操作", postback.Uname);
            return RedirectToAction("Edit", "Tables");
        }
        /// ///////////////////////////DELETE////////////////////////////////////////////////////////////////////////////////////////
        // GET: Tables/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Tables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            User user = db.User.Find(id);
            db.User.Remove(user);
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
