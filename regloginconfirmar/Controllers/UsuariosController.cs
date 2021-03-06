﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using regloginconfirmar.Models;
using System.Net.Mail;
using System.Net;
using regloginconfirmar.Models.ModelViews;
using System.Web.Security;
using regloginconfirmar.helpers;

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
        public void EnviarVerificacionCorreoLink (string email, string cod_activacion, string emailPara = "ActivarCuenta")
        {
            var verifyUrl = "/Usuarios/"+emailPara+"/" + cod_activacion;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("zealito@gmail.com","Prueba CodVerificacion");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = ""; //clave del correo

            string subject = "";
            string body = "";
            if (emailPara == "ActivarCuenta")
            {
                subject = "Su cuenta fue creada con exito";

                body = "<br/><br/> Cuerpo del mensaje del correo" +
                    "Cuenta creada con exito, para usar su cuenta, activelo y verifique su cuennta" +
                    "<br/><br/>  <a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailPara == "ResetearPassword")
            {
                subject = "Resetear Contraseña";

                body = "<br/><br/> Recuperación de Contraseña" +
                    "<br/><br/> Para resetear su contraseña, haga click " +
                    "<br/><br/>  <a href=" + link + ">Resetear Contraseña</a> ";
            }
           
          

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

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UsuarioLogin model, string ReturnUrl="")
        {
            string message = "";
            using (registrodbEntities db = new registrodbEntities())
            {
                var user = db.Usuario.Where(u => u.Email == model.Email).FirstOrDefault();
                if (user != null)
                {
                    if (string.Compare(helpers.Encriptar.Hash(model.Password),user.Password)==0)
                    {
                        int timeout = model.RememberMe ? 43800 : 1;  //un mes = 43800
                        var ticket = new FormsAuthenticationTicket(model.Email, model.RememberMe, timeout);
                        string encryptar = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptar);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index","Home");
                        }
                    }
                }
                else
                {
                    message = "Error de Autentificación";
                }

            }
            ViewBag.Message = message;
            return View();
        }


        [HttpGet]
        public ActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecuperarPassword(string Email)
        {
            string message = "";
            bool status = false;

            using (registrodbEntities db = new registrodbEntities())
            {
                var cuenta = db.Usuario.Where(u => u.Email == Email).FirstOrDefault();
                if (cuenta != null)
                {
                    string resetCodigo = Guid.NewGuid().ToString();
                    EnviarVerificacionCorreoLink(cuenta.Email, resetCodigo, "ResetearPassword");
                    cuenta.Cod_Recuperacion = resetCodigo;

                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();

                    message = "Para resetear su cuenta, el Link se envio a su correo";
                }
                else
                {
                    message = "La cuenta no existe";
                }

            }
            ViewBag.Message = message;
           return View();
        }


        public ActionResult ResetearPassword(string id)
        {
            using (registrodbEntities db = new registrodbEntities())
            {
                var usuario = db.Usuario.Where(u => u.Cod_Recuperacion == id).FirstOrDefault();
                if (usuario != null)
                {
                    UsuarioResetPass model = new UsuarioResetPass();
                    model.Cod_Reset = id;

                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetearPassword(UsuarioResetPass model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (registrodbEntities db = new registrodbEntities())
                {
                    var usuario = db.Usuario.Where(u => u.Cod_Recuperacion == model.Cod_Reset).FirstOrDefault();
                    if (usuario != null)
                    {
                        usuario.Password =Encriptar.Hash(model.NuevaPassword);
                        usuario.Cod_Recuperacion = "";
                        db.Configuration.ValidateOnSaveEnabled = false;
                        db.SaveChanges();

                        message = "Nueva contraseña se ha generado con exito";
                    }
                }
            }
            else
            {
                message = "Error en Resetear su Contraseña";
            }
            ViewBag.Message = message;
            return View(model);
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login","Usuarios");
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
