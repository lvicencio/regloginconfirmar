using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using regloginconfirmar.Models;
using System.Net.Mail;
using System.Net;

namespace regloginconfirmar.Controllers
{
    public class UsuariosController : Controller
    {
        [HttpGet]
        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar([Bind(Exclude = "Email_Verificacion,Cod_Activacion")]Usuario user)
        {
            bool Status = false;
            string message = "";
            //validacion del modelo
            if (ModelState.IsValid)
            {
                //Ver si el correo existe

                var correoexiste = VerificarCorreo(user.Email);
                if (correoexiste)
                {
                    ModelState.AddModelError("CorreoExiste", "El Correo ya se encuentra Registrado");
                    return View(user);
                }
                //generacion del codigo de activacion
                user.Cod_Activacion = Guid.NewGuid();

                //Encriptar password
                user.Password = helpers.Encriptar.Hash(user.Password);
                user.ConfirmPassword = helpers.Encriptar.Hash(user.ConfirmPassword);

                user.Email_Verificacion = false;

                //guardar data

                using (registrodbEntities db = new registrodbEntities())
                {
                    db.Usuario.Add(user);
                    db.SaveChanges();

                    //enviar correo de activacion del usuario
                    EnviarVerificacionCorreoLink(user.Email, user.Cod_Activacion.ToString());
                    message = "Registracion exitosa. Debe activar su cuenta, "+
                        "Se ha enviado un link de activación a su correo "+ user.Email;
                    Status = true;
                }

            }
            else
            {
                message = "Solicitud Invalida";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;

            return View(user);
        }

        [HttpGet]
        public ActionResult ActivarCuenta(string id)
        {
            bool Status = false;
            using (registrodbEntities db = new registrodbEntities())
            {
                db.Configuration.ValidateOnSaveEnabled = false;  //ihabilitar confirmacion pass
                var vr = db.Usuario.Where(u => u.Cod_Activacion == new Guid(id)).FirstOrDefault();
                if (vr != null)
                {
                    vr.Email_Verificacion = true;
                    db.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Respuesta Invalida";
                }

            }
            ViewBag.Status = Status;
            return View();
        }

        [NonAction]
        public bool VerificarCorreo(string email)
        {
            using (registrodbEntities db = new registrodbEntities())
            {
                var ver = db.Usuario.Where(x => x.Email == email).FirstOrDefault();
                //return ver == null ? false : true;
                return ver != null;
            }          
        }

        [NonAction]
        public void EnviarVerificacionCorreoLink (string email, string cod_activacion)
        {
            var verifyUrl = "/Usuarios/ActivarCuenta/" + cod_activacion;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("zealito@gmail.com","Prueba CodVerificacion");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = ""; //clave del correo
            string subject = "Su cuenta fue creada con exito";

            string body = "<br/><br/> Cuerpo del mensaje del correo" +
                "Cuenta creada con exito, para usar su cuenta, activelo y verifique su cuennta" +
                "<br/><br/>  <a href='"+link+"'>"+link+"</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
             };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })

                smtp.Send(message);

        }



        // GET: Usuarios
        public ActionResult Index()
        {
            return View();
        }

        // GET: Usuarios/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Usuarios/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
