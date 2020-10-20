using LoginPage.LoginData.DbData;
using LoginPage.Models;
using System.IO;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;
using Microsoft.Ajax.Utilities;

namespace LoginPage.Controllers
{

    public class UserController : Controller
    {
        DbData lodinDb = new DbData();

        public ActionResult Index()
        {
            var user = (DbData)Session["DbData"];
            if (user!= null)
            {
                string email = Session["UserMail"].ToString();
                string[] data = lodinDb.GetUsers(email);


                var obj = new Registration()
                {
                    UserId = Int32.Parse(data[0]),
                    FullName = data[1],
                    Email = email,
                    Cvpath = data[3]
                };
                
                return View(obj);
            }
            else {
                return RedirectToAction("Login");
            }
            
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(Registration registration)
        {
            if (registration.Cv != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(registration.Cv.FileName);
                string extention = Path.GetExtension(registration.Cv.FileName);
                string newfilename = registration.FullName + extention;
                registration.Cvpath = "~/Cv/" + newfilename;
                registration.Cv.SaveAs(Path.Combine(Server.MapPath("~/Cv/"),newfilename));
            }
            byte[] temp;
            SHA1 sha = new SHA1CryptoServiceProvider();
            temp = sha.ComputeHash(Encoding.UTF8.GetBytes(registration.Password));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < temp.Length; i++)
            {
                sb.Append(temp[i].ToString("x2"));
            }

            string hash = sb.ToString();
            Registration user = new Registration()
            {
                FullName = registration.FullName,
                Email = registration.Email,
                Password = hash,
                Cvpath = registration.Cvpath,

            };
            lodinDb.Save(user.FullName, user.Email, user.Password, user.Cvpath);

            return Content(user.FullName);
        }


        public ActionResult Hello(string name)
        {
            Registration registration = new Registration()
            {
                FullName = name
            };
            return View(registration);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Userlogin user)
        {
            var obj = new Userlogin();
            obj.Email = user.Email;
            obj.Password = user.Password;

            byte[] temp;
            SHA1 sha = new SHA1CryptoServiceProvider();
            temp = sha.ComputeHash(Encoding.UTF8.GetBytes(user.Password));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < temp.Length; i++)
            {
                sb.Append(temp[i].ToString("x2"));
            }

            string hash = sb.ToString();

            var psw = lodinDb.LogUser(obj.Email);

            psw = psw.Replace(" ", String.Empty);
            if (psw != null)
            {
                if (psw.Equals(hash))
                {
                  string[] arr= {hash,obj.Email };
                    
                    Session["UserMail"] = user.Email;
                    return Json(new {Items=arr});
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        
        public ActionResult Edit(int id,string name,string email,string cv) {
            var obj = new Registration()
            {
                UserId = id,
                FullName = name,
                Email = email,
                Cvpath = cv
            };

            return View(obj);
        }
        [HttpPost]
        public ActionResult Edit(Registration registration) {
            if (registration.Cvpath!= null) {
                string exfilename= Server.MapPath(registration.Cvpath);
                if ((System.IO.File.Exists(exfilename)))
                {
                    System.IO.File.Delete(exfilename);
                }
               
                string fileName = Path.GetFileNameWithoutExtension(registration.Cv.FileName);
                string extention = Path.GetExtension(registration.Cv.FileName);
                string newfilename = registration.FullName + extention;
                registration.Cvpath = "~/Cv/" + newfilename;
                registration.Cv.SaveAs(Path.Combine(Server.MapPath("~/Cv/"), newfilename));
            }

            Registration modifieddata = new Registration() {
            UserId=registration.UserId,
            FullName=registration.FullName,
            Email=registration.Email,
            Cvpath=registration.Cvpath
            };
            lodinDb.modifie(modifieddata.UserId, modifieddata.FullName, modifieddata.Email, modifieddata.Cvpath);

            return Content(registration.Email);
        }

        public string UserlogState() {
            if (Session["UserMail"] != null) {
                return "true";
            }
            else {
                return "false";
            }
        }
        public bool UserlogOut() {
            //Session.Remove("UserMail");
            Session.Abandon();
            return true;
        }
    }

}