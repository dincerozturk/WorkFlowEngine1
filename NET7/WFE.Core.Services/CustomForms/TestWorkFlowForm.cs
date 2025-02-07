﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
//using System.Web.Mvc;
using WorkFlowManager.Common.Constants;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Helper;
using WorkFlowManager.Services.DbServices;

namespace WorkFlowManager.Services.CustomForms
{
    public class TestWorkFlowForm : ITestWorkFlowForm
    {
        private readonly ITestWorkFlowProcessService _testWorkFlowProcessService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationHelper _validationHelper;
        private readonly IMapper _mapper;

        public TestWorkFlowForm(IUnitOfWork unitOfWork, ITestWorkFlowProcessService testWorkFlowProcessService, IValidationHelper validationHelper)
        {
            _unitOfWork = unitOfWork;
            _testWorkFlowProcessService = testWorkFlowProcessService;
            _validationHelper = validationHelper;
        }

        public void Save(WorkFlowFormViewModel formData)
        {
            _testWorkFlowProcessService.WorkFlowFormSave<TestForm, TestWorkFlowFormViewModel>(formData);
        }

        public bool Validate(WorkFlowFormViewModel formData, ModelStateDictionary modelState)
        {
            return _validationHelper.Validate((TestWorkFlowFormViewModel)formData, new TestWorkFlowFormViewModelValidator(_unitOfWork), modelState);
        }

        public WorkFlowFormViewModel Load(WorkFlowFormViewModel workFlowFormViewModel)
        {
            TestForm testForm = _unitOfWork.Repository<TestForm>().Get(x => x.OwnerId == workFlowFormViewModel.OwnerId);

            if (testForm == null)
            {
                testForm = new TestForm();
                testForm.OwnerId = workFlowFormViewModel.OwnerId;
                _unitOfWork.Repository<TestForm>().Add(testForm);
                _unitOfWork.Complete();
            }
            TestWorkFlowFormViewModel testFormViewModel = _mapper.Map<TestForm, TestWorkFlowFormViewModel>(testForm);
            _mapper.Map(workFlowFormViewModel, testFormViewModel);

            return testFormViewModel;
        }
    }
}
