using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class OrdersController : Controller
    {
        private Database1Entities1 db = new Database1Entities1();

        ////////////顯示所有訂單/////////////////////////////////////////////////////////////////////////////////////

        // GET: Orders
        public ActionResult Index()
        {
            string Account = this.User.Identity.Name;  //使用者
            ViewBag.Account = Account;

            var user = (from a in db.User
                         where a.Account == Account
                         select a).FirstOrDefault();

            IEnumerable<Order> order = (from o in db.Order
                         where o.O_Account == user.Account &&o.Payed==0
                         select o).ToList();

            return View(order);

        }

        /////////////顯示所有歷史訂單///////////////////////////////////////////////////////////////////////////////////

        //歷史訂單
        public ActionResult OldIndex()
        {
            string Account = this.User.Identity.Name;  //使用者
            ViewBag.Account = Account;

            var user = (from a in db.User
                        where a.Account == Account
                        select a).FirstOrDefault();

            IEnumerable<Order> order = (from o in db.Order
                                        where o.O_Account == user.Account && o.Payed == 1
                                        select o).ToList();
            return View(order);
        }

        /////////////結帳///////////////////////////////////////////////////////////////////////////////////

        //歷史訂單
        public ActionResult pay()
        {
            string Account = this.User.Identity.Name;  //使用者
            ViewBag.Account = Account;

            var user = (from a in db.User
                        where a.Account == Account
                        select a).FirstOrDefault();

            IEnumerable<Order> order = (from o in db.Order
                                        where o.O_Account == user.Account && o.Payed == 0
                                        select o).ToList();
            foreach (var o in order)
            {
                o.Payed = 1;
            }
            db.SaveChanges();

            return RedirectToAction("OldIndex", "orders",order);
        }
        // GET: Orders/Details/5
        /////////////訂單詳細內容///////////////////////////////////////////////////////////////////////////////////
        public ActionResult Details(Guid? Oid)
        {
            if (Oid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = (from a in db.Order
                           where a.Oid == Oid
                           select a).FirstOrDefault();
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }


        /////////////新增訂單///////////////////////////////////////////////////////////////////////////////
        // GET: Orders/Create
        [Authorize]
        public ActionResult Create(CreatOrder CreatOrder,Guid Pid)
        {
            string Account = this.User.Identity.Name;
            User user= (from a in db.User
                        where a.Account == Account
                        select a).FirstOrDefault();
            if (user == null)
            {
                ViewBag.message = "請登入會員";
                return View();
            }
            ViewBag.Account = user.Account;
            // IEnumerator Pid = CreatOrder.Pid.GetEnumerator();
            //   Guid Pid= IEnumerator GetEnumerator(CreatOrder.Pid);
            Product product = (from a in db.Product
                               where a.Pid == Pid
                               select a).FirstOrDefault();
            if (product == null)
            {
                ViewBag.message = "讀取不到產品資料";
                return View();
            }
            ViewBag.Pname = product.Pname;
            DateTime dt = DateTime.Now; //取得目前日期時間
            ViewBag.dt =dt;
            return View();
        }

        // POST: Orders/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Oid,O_Account,O_Pid,Num,Sum,Payed,Date")] Order order, CreatOrder CreatOrder, Guid Pid)
        {

            ///讀取不須輸入的資料
            string Account = this.User.Identity.Name;
            User user = (from a in db.User
                         where a.Account == Account
                         select a).FirstOrDefault();
            if (user == null)
            {
                ViewBag.message = "請登入會員";
                return View();
            }
            // IEnumerator Pid = CreatOrder.Pid.GetEnumerator();
            //   Guid Pid= IEnumerator GetEnumerator(CreatOrder.Pid);
            Product product = (from a in db.Product
                               where a.Pid == Pid
                               select a).FirstOrDefault();
            if (product == null)
            {
                ViewBag.message = "讀取不到產品資料";
                return View();
            }
            DateTime dt = DateTime.Now; //取得目前日期時間
            //////讀取需要輸入的資料並存回//////////////////////////////////////////////////////
            if (ModelState.IsValid)
            {
                order.Oid = Guid.NewGuid();  //訂單編號
                order.O_Account = Account;  //使用者
                order.O_Pid = Pid;  //買了甚麼
                //ViewBag.Pname = product.Pname;
                order.Num = CreatOrder.FormNum;//數量多少
                order.Date = dt.ToString();
                order.Payed = 0;
                order.Sum = product.Price * CreatOrder.FormNum;
                product.Hot = product.Hot + 1;
                db.Order.Add(order);

                db.SaveChanges();
                return RedirectToAction("Index","orders");
            }

            ViewBag.O_Pid = new SelectList(db.Product, "Pid", "Show", order.O_Pid);
            ViewBag.O_Account = new SelectList(db.User, "Account", "Password", order.O_Account);
            return View(order);
        }

        /////////////修改訂單///////////////////////////////////////////////////////////////////////////////
        // GET: Orders/Create
        // GET: Orders/Edit/5
        public ActionResult Edit(Guid Oid)
        {
            if (Oid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order= (from a in db.Order
                          where a.Oid == Oid
                          select a).FirstOrDefault();
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.O_Pid = new SelectList(db.Product, "Pid", "Show", order.O_Pid);
            ViewBag.O_Account = new SelectList(db.User, "Account", "Password", order.O_Account);
            return View(order);
        }

        // POST: Orders/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Models.Order postback)
        {
            var result = (from s in db.Order
                          where s.Oid == postback.Oid
                          select s).FirstOrDefault();
            Product product = (from s in db.Product
                          where s.Pid == postback.O_Pid
                          select s).FirstOrDefault();
           
            if (result != default(Models.Order))
            {
                result.Num = postback.Num;
                result.Sum = product.Price * postback.Num;
                    db.SaveChanges();
                    return RedirectToAction("Index");
            }

            return View(postback.Oid);
        }

        ///////////////////////刪除訂單//////////////////////////////////////////////////////////////////////////////////////////


        // GET: Orders/Delete/5
        public ActionResult Delete(Guid Oid)
        {
            if (Oid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = (from a in db.Order
                           where a.Oid == Oid
                           select a).FirstOrDefault();
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid Oid)
        {
            Order order = (from a in db.Order
                           where a.Oid == Oid
                           select a).FirstOrDefault();
            db.Order.Remove(order);
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
