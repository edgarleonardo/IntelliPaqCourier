﻿using IntelliPack.DataAccessLayer.DataManagers;
using IntelliPack.DataAccessLayer.Models;
using IntelliPackWeb.Base;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntelliPackWeb.Controllers
{
    public class PackagesController : BaseController
    {
        [Authorize]
        public ActionResult Add(Packages model)
        {
            try
            {
                getCookies();
                PackagesManager manager = new PackagesManager();
                manager.Set(model);

                ViewBag.Success = "Datos Actualizados Satisfactoriamente";
                return Content("Datos Actualizados Satisfactoriamente");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return Content(ViewBag.Error);
        }
        [Authorize]
        public ActionResult Delete(Packages model)
        {
            try
            {
                getCookies();
                PackagesManager manager = new PackagesManager();
                /// Status de eliminado
                model.status_code = 2;
                manager.Set(model);

                ViewBag.Success = "Datos Eliminado Satisfactoriamente";
                return Content("Datos Eliminado Satisfactoriamente");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return Content(ViewBag.Error);
        }
        [Authorize]
        public ActionResult PackageActived()
        {
            getCookies();
            PackagesManager manager = new PackagesManager();
            var result = manager.GetActived();
            ViewBag.PackageTitles = "Paquetes Activos Clientes";
            return View("Packages", result);
        }
        [Authorize]
        public ActionResult PackageinActived()
        {
            getCookies();
            PackagesManager manager = new PackagesManager();
            var result = manager.GetinActived();
            ViewBag.PackageTitles = "Paquetes inActivos Clientes";
            return View("Packages", result);
        }
        [Authorize]
        public ActionResult PackageById(int CustId)
        {
            getCookies();
            if (userIdLogged != CustId)
            {
                CustId = userIdLogged;
            }
            PackagesManager manager = new PackagesManager();
            var result = manager.GetUserId(CustId);
            ViewBag.PackageTitles = "Paquetes Activos Cliente";
            return View("Packages", result);
        }
        [Authorize]
        public ActionResult PackageByIdHistorico(int CustId)
        {
            getCookies();
            if (userIdLogged != CustId)
            {
                CustId = userIdLogged;
            }
            PackagesManager manager = new PackagesManager();
            var result = manager.GetHistoryUserId(CustId);
            ViewBag.PackageTitles = "Paquetes Historicos";
            return View("Packages", result);
        }
        [Authorize]
        public ActionResult Get(string Id, string partial_view)
        {
            getCookies();
            PackagesManager manager = new PackagesManager();
            var result = manager.GetById(Id, userIdLogged);
            return PartialView(partial_view, result);
        }
        [Authorize]
        public ActionResult SeleccionarCourier()
        {
            getCookies();
            ViewBag.Couriers = GetDrpCourier();
            return View();
        }

        [Authorize]
        public ActionResult ApplyPagos(int no_id)
        {
            getCookies();
            var model = new Pagos() { no_id = no_id };
            PagosManager pagos = new PagosManager();
            pagos.HacerPago(model, userIdLogged);
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ApplyPagos(Pagos model)
        {
            getCookies();
            PagosManager pagos = new PagosManager();
            pagos.HacerPago(model, userIdLogged);
            return RedirectToAction("ModuloPagoCouriers");
        }
        [Authorize]
        public ActionResult ModuloPagoCouriers()
        {
            getCookies();
            CuotasManager cuotas = new CuotasManager();
            var result = cuotas.GetCuotas(userIdLogged);
                   
            return View(result);
        }
        public ActionResult RunCuotas()
        {
            getCookies();
            CuotasManager cuotas = new CuotasManager();
            cuotas.CorrerPagos(userIdLogged);
            return RedirectToAction("ModuloPagoCouriers");            
        }
        
        // GET: Packages
        [Authorize]
        public ActionResult PackageUploadBatch()
        {
            try
            {
                getCookies();
                if (Request != null)
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength >= 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        var packagesList = new List<Packages>();

                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;

                            if (noOfRow >= 2)
                            {
                                PackagesManager pk = new PackagesManager();
                                var result = pk.SetWorkFlow();
                                int workflowId = 0;
                                if (result != null)
                                {
                                    workflowId = result.workflowid;
                                }
                                if (workflowId > 0)
                                {
                                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        if (workSheet.Cells[rowIterator, 1].Value != null &&
                                            workSheet.Cells[rowIterator, 1].Value.ToString().Trim() != "")
                                        {
                                            var packageInfo = new Packages();
                                            packageInfo.tracking_code = workSheet.Cells[rowIterator, 1].Value.ToString();
                                            packageInfo.correo = workSheet.Cells[rowIterator, 2].Value.ToString();
                                            decimal peso = 0;
                                            decimal.TryParse(workSheet.Cells[rowIterator, 3].Value.ToString(), out peso);
                                            packageInfo.peso = peso;
                                            packageInfo.workflowid = workflowId;
                                            packageInfo.WH = workSheet.Cells[rowIterator, 4].Value.ToString();
                                            packageInfo.consignado = workSheet.Cells[rowIterator, 5].Value.ToString();
                                            packageInfo.contenido = workSheet.Cells[rowIterator, 6].Value.ToString();
                                            packageInfo.tienda = workSheet.Cells[rowIterator, 7].Value.ToString();
                                            packageInfo.origen = workSheet.Cells[rowIterator, 8].Value.ToString();
                                            decimal manejo = 0;
                                            decimal.TryParse(workSheet.Cells[rowIterator, 9].Value.ToString(), out manejo);
                                            packageInfo.manejo = manejo;
                                            decimal costoXlibra = 0;
                                            decimal.TryParse(workSheet.Cells[rowIterator, 10].Value.ToString(), out costoXlibra);
                                            packageInfo.manejo = costoXlibra;
                                            packagesList.Add(packageInfo);
                                            pk.Set(packageInfo);
                                        }                                        
                                    }
                                }                                
                            }
                        }
                        Set_Message("Datos Guardados Correctamente");
                    }
                }
                
                return View();
            }
            catch (Exception ex)
            {
                Set_Message("Ha Ocurrido Un Error: " + ex.Message);
            }
            return View();
        }

        
    }
}