using GP.Petstar.Web.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace GP.Petstar.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            
            return View();
        }

        public JsonResult SendMail()
        {
            var response = new MethodResponse<string> { code = 0 };
            try
            {

                var user = User.Identity.Name;

                #region formatter

                var hostname = Request.Url.Host;

                var url = (Request.Url.IsDefaultPort) ? Request.Url.Host : Request.Url.Host + ":" + Request.Url.Port;

                String Msg_HTMLx = @"<html>
	                                <body>
                                        <a href='https://www.petstar.mx/'>
                                            <img src = 'http://" + url + @"/images/C2.jpg' class='image' style='max-width: 100%;' />
			                            </a>
                                        <a href='https://www.petstar.mx/petstar/contacto/'>
                                            <img src = 'http://" + url + @"/images/2.jpg' class='image' style='max-width: 100%;' />
			                            </a>
	                                </body>
                                </html>";
                #endregion


                var emailControl = new EmailControl();

                var result = emailControl.SendMail(Properties.Settings.Default.Email_From, user, "Fin del recorrido!!", true, Msg_HTMLx, Properties.Settings.Default.Email_Host, Convert.ToInt32(Properties.Settings.Default.Email_Port), Properties.Settings.Default.Email_User, Properties.Settings.Default.Email_Pass, true, true);

                if (result.code != 0)
                    throw new Exception("Error al enviar email.");

            }
            catch (Exception ex)
            {
                response.code = -100;
                response.message = ex.Message;
                
            }
            return Json(response, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }
    }
}
