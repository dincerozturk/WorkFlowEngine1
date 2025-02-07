﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
//using System.Web.Mvc;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Extensions;
using WorkFlowManager.Common.InfraStructure;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Web.Controllers
{

    public class WorkFlowProcessController : Controller
    {

        private readonly IWorkFlowProcessService _workFlowProcessService;
        private readonly IWorkFlowService _workFlowService;
        private readonly IMapper _mapper;

        public WorkFlowProcessController(
            IWorkFlowProcessService workFlowProcessService
            , IWorkFlowService workFlowService
            , IMapper mapper

            )
        {
            _workFlowProcessService = workFlowProcessService;
            _workFlowService = workFlowService;
            _mapper = mapper;
        }

        public IActionResult GetProcess(int ownerId, int taskId)
        {
            var lastProcessId = _workFlowProcessService.GetLastProcessId(ownerId);

            if (lastProcessId != 0)
            {
                return Index(lastProcessId);
            }
            return StartWorkFlow(ownerId, taskId);
        }

        public IActionResult StartWorkFlow(int ownerId, int taskId)
        {
            int workFlowTraceId = _workFlowProcessService.StartWorkFlow(ownerId, taskId);
            return Index(workFlowTraceId);
        }

        [HttpPost]
        public IActionResult CancelProcess(int workFlowTraceId)
        {
            _workFlowProcessService.WorkFlowProcessCancel(workFlowTraceId);

            var surecKontrolSonuc = _workFlowProcessService.SetNextProcessForWorkFlow(workFlowTraceId);

            return RedirectToAction("Index", surecKontrolSonuc).WithMessage(this, "Process cancelled.", MessageType.Success);

        }


        public IActionResult ShowWorkFlow(int workFlowTraceId)
        {
            UserProcessViewModel userProcessVM = _workFlowProcessService.GetUserProcessVM(workFlowTraceId);
            string workFlowDiagram = _workFlowService.GetWorkFlowDiagram(userProcessVM.TaskId);
            var workFlow = _workFlowProcessService.GetWorkFlow(workFlowDiagram, workFlowTraceId);
            return PartialView("_ShowWorkflowPartial", new WorkFlowView { Flag = true, WorkFlowText = workFlow });
        }

        public IActionResult Index(int workFlowTraceId)
        {
            UserProcessViewModel userProcessVM = _workFlowProcessService.GetUserProcessVM(workFlowTraceId);
            if (_workFlowProcessService.WorkFlowPermissionCheck(userProcessVM) == false)
            {
                return RedirectToAction("Index", new { controller = "Home" }).WithMessage(this, "Access denied!", MessageType.Danger);
            }

            WorkFlowTrace workFlowTrace = _workFlowProcessService.WorkFlowTraceDetail(workFlowTraceId);

            WorkFlowFormViewModel workFlowTraceForm = _mapper.Map<WorkFlowTrace, WorkFlowFormViewModel>(workFlowTrace);
            int ownerId = workFlowTraceForm.OwnerId;

            var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(userProcessVM);
            _workFlowProcessService.SetWorkFlowTraceForm(workFlowTraceForm, workFlowBase);

            var workFlowForm = _workFlowProcessService.WorkFlowFormLoad(workFlowTraceForm);

            var viewResult = View(workFlowForm.ProcessTaskSpecialFormTemplateView, workFlowForm);
            return viewResult;

        }

        [HttpPost]
        //[MultipleButton(Name = "action", Argument = "ReturnTo")]
        public IActionResult ReturnTo(WorkFlowFormViewModel formData)
        {
            if (formData.Description == null)
            {
                string errorMessage = "Please enter your cancellation reason.";
                ModelState.AddModelError("ProcessForm.Description", errorMessage);
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, "Error occured!", MessageType.Warning);
            }

            WorkFlowTrace islem = _mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);
            _workFlowProcessService.AddOrUpdate(islem);

            var workFlowTraceId = formData.Id;
            _workFlowProcessService.CancelWorkFlowTrace(workFlowTraceId, formData.TargetProcessId);

            var surecKontrolSonuc = _workFlowProcessService.SetNextProcessForWorkFlow(workFlowTraceId);

            return RedirectToAction("Index", surecKontrolSonuc).WithMessage(this, "Previous step started.", MessageType.Success);

        }

        [HttpPost]
        //[MultipleButton(Name = "action", Argument = "SaveAsDraft")]
        public IActionResult SaveAsDraft(WorkFlowFormViewModel formData)
        {
            if (ModelState.IsValid)
            {
                if (formData.ProcessFormViewCompleted && formData.GetType() != typeof(WorkFlowFormViewModel))
                {
                    _workFlowProcessService.CustomFormSave(formData);
                }
                else
                {

                    WorkFlowTrace torSatinAlmaIslem = _mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);
                    _workFlowProcessService.AddOrUpdate(torSatinAlmaIslem);
                }

                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);


                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("{0} saved successfully.", formData.ProcessName), MessageType.Success);
            }
            else
            {
                UserProcessViewModel kullaniciIslemVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(kullaniciIslemVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("Validation error!"), MessageType.Danger);
            }
        }

        [HttpPost]
        //[MultipleButton(Name = "action", Argument = "SaveAndSend")]
        public IActionResult SaveAndSend(WorkFlowFormViewModel formData)
        {

            if (ModelState.IsValid)
            {

                bool fullFormValidate = _workFlowProcessService.FullFormValidate(formData, ModelState);

                if (!fullFormValidate)
                {

                    UserProcessViewModel userProcessVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                    var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(userProcessVM);
                    _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                    return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, "Validation error!", MessageType.Warning);
                }

                WorkFlowTrace workFlowTrace = _mapper.Map<WorkFlowFormViewModel, WorkFlowTrace>(formData);

                if (formData.ProcessFormViewCompleted)
                {
                    if (formData.GetType() != typeof(WorkFlowFormViewModel))
                    {
                        _workFlowProcessService.CustomFormSave(formData);
                    }
                    else
                    {
                        _workFlowProcessService.AddOrUpdate(workFlowTrace);
                    }
                }
                else
                {
                    _workFlowProcessService.AddOrUpdate(workFlowTrace);
                }


                _workFlowProcessService.GoToWorkFlowNextProcess(workFlowTrace.OwnerId);
                var targetProcess = _workFlowProcessService.SetNextProcessForWorkFlow(workFlowTrace.Id);

                return RedirectToAction("Index", targetProcess).WithMessage(this, "Saved Successfully.", MessageType.Success);
            }
            else
            {
                UserProcessViewModel userProcessVM = _workFlowProcessService.GetUserProcessVM(formData.Id);
                var workFlowBase = _workFlowProcessService.WorkFlowBaseInfo(userProcessVM);
                _workFlowProcessService.SetWorkFlowTraceForm(formData, workFlowBase);

                return View(formData.ProcessTaskSpecialFormTemplateView, formData).WithMessage(this, string.Format("Validation error!"), MessageType.Danger);
            }
        }

    }
}

