﻿namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Collections;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Controllers;

    using ModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationViewModel;
    using GridModelType = OJS.Web.Areas.Administration.ViewModels.Submission.SubmissionAdministrationGridViewModel;

    public class SubmissionsController : KendoGridAdministrationController
    {
        private const string SuccessfulCreationMessage = "Решението беше добавено успешно!";
        private const string SuccessfulEditMessage = "Решението беше променено успешно!";
        private const string InvalidSubmissionMessage = "Невалидно решение!";
        private const string RetestSuccessful = "Решението беше успешно пуснато за ретестване!";

        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        public override IEnumerable GetData()
        {
            return this.Data.Submissions
                .All()
                .Select(GridModelType.ViewModel);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ModelType model)
        {
            if (model != null && ModelState.IsValid)
            {
                this.BaseCreate(new DataSourceRequest(), model.ToEntity);

                this.TempData["InfoMessage"] = SuccessfulCreationMessage;
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .Where(subm => subm.Id == id)
                .Select(ModelType.ViewModel)
                .FirstOrDefault();

            if (submission == null)
            {
                TempData["DangerMessage"] = InvalidSubmissionMessage;
                this.RedirectToAction("Index");
            }

            return this.View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ModelType model)
        {
            if (model != null && ModelState.IsValid)
            {
                this.BaseUpdate(new DataSourceRequest(), model.ToEntity);

                this.TempData["InfoMessage"] = SuccessfulEditMessage;
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .Where(subm => subm.Id == id)
                .Select(GridModelType.ViewModel)
                .FirstOrDefault();

            if (submission == null)
            {
                TempData["DangerMessage"] = InvalidSubmissionMessage;
                this.RedirectToAction("Index");
            }

            return this.View(submission);
        }
         
        public ActionResult ConfirmDelete(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .FirstOrDefault(subm => subm.Id == id);

            if (submission == null)
            {
                TempData["DangerMessage"] = InvalidSubmissionMessage;
                this.RedirectToAction("Index");
            }

            foreach (var testRun in submission.TestRuns.ToList())
            {
                this.Data.TestRuns.Delete(testRun.Id);
            }

            this.Data.Submissions.Delete(id);
            this.Data.SaveChanges();

            return this.RedirectToAction("Index");
        }

        public JsonResult GetSubmissionTypes()
        {
            var dropDownData = this.Data.SubmissionTypes
                .All()
                .ToList()
                .Select(subm => new SelectListItem
                {
                    Text = subm.Name,
                    Value = subm.Id.ToString(),
                });

            return Json(dropDownData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Retest(int id)
        {
            var submission = this.Data.Submissions
                .All()
                .FirstOrDefault(subm => subm.Id == id);

            if (submission == null)
            {
                TempData["DangerMessage"] = InvalidSubmissionMessage;
            }
            else
            {
                submission.Processed = false;
                submission.Processing = false;
                this.Data.SaveChanges();

                TempData["InfoMessage"] = RetestSuccessful;
            }

            return this.RedirectToAction("View", "Submissions", new { area = "Contests", id = id });
        }

        public JsonResult GetProblems(string text)
        {
            var dropDownData = this.Data.Problems.All();

            if (!string.IsNullOrEmpty(text))
            {
                dropDownData = dropDownData.Where(pr => pr.Name.ToLower().Contains(text.ToLower()));
            }

            var result = dropDownData
                .ToList()
                .Select(pr => new SelectListItem
                {
                    Text = pr.Name,
                    Value = pr.Id.ToString(),
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParticipants(string text, int problem)
        {
            var selectedProblem = this.Data.Problems.All().FirstOrDefault(pr => pr.Id == problem);

            var dropDownData = this.Data.Participants.All().Where(part => part.ContestId == selectedProblem.ContestId);

            if (!string.IsNullOrEmpty(text))
            {
                dropDownData = dropDownData.Where(part => part.User.UserName.ToLower().Contains(text.ToLower()));
            }

            var result = dropDownData
                .ToList()
                .Select(part => new SelectListItem
                {
                    Text = part.User.UserName,
                    Value = part.Id.ToString(),
                });

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}