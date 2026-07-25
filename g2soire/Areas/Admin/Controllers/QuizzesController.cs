using FluentValidation;
using MaisonAeternum.Application.Assessment.Abstractions;
using MaisonAeternum.Application.Assessment.Models;
using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaisonAeternum.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class QuizzesController : Controller
{
    private readonly IQuizService _quizService;
    private readonly IModuleService _moduleService;
    private readonly IValidator<QuizFormDto> _quizValidator;
    private readonly IValidator<QuestionFormDto> _questionValidator;

    public QuizzesController(
        IQuizService quizService,
        IModuleService moduleService,
        IValidator<QuizFormDto> quizValidator,
        IValidator<QuestionFormDto> questionValidator)
    {
        _quizService = quizService;
        _moduleService = moduleService;
        _quizValidator = quizValidator;
        _questionValidator = questionValidator;
    }

    public async Task<IActionResult> ForModule(int moduleId, CancellationToken cancellationToken)
    {
        var quiz = await _quizService.GetForModuleAsync(moduleId, cancellationToken);
        if (quiz is not null) return RedirectToAction(nameof(Edit), new { id = quiz.Id });

        var module = await _moduleService.GetForEditAsync(moduleId, cancellationToken);
        if (module is null) return NotFound();

        return View("CreateForModule", new QuizFormDto
        {
            ModuleId = moduleId,
            FormationId = module.FormationId,
            Type = Domain.Enums.QuizType.ModuleQuiz,
            Title = $"{module.Title} — Module Quiz"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateForModule(QuizFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _quizValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        var id = await _quizService.CreateForModuleAsync(form.ModuleId!.Value, form.FormationId, form, cancellationToken);

        TempData["ToastMessage"] = "Quiz created — now add its questions.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> ForFormation(int formationId, CancellationToken cancellationToken)
    {
        var quiz = await _quizService.GetFinalExamForFormationAsync(formationId, cancellationToken);
        if (quiz is not null) return RedirectToAction(nameof(Edit), new { id = quiz.Id });

        return View("CreateFinalExam", new QuizFormDto
        {
            FormationId = formationId,
            Type = Domain.Enums.QuizType.FinalExam,
            Title = "Bench Trial — Final Exam"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFinalExam(QuizFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _quizValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        var id = await _quizService.CreateFinalExamAsync(form.FormationId, form, cancellationToken);

        TempData["ToastMessage"] = "Final exam created — now add its questions.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var form = await _quizService.GetForEditAsync(id, cancellationToken);
        if (form is null) return NotFound();

        ViewBag.Questions = await _quizService.GetQuestionsAsync(id, cancellationToken);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuizFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _quizValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid)
        {
            ViewBag.Questions = await _quizService.GetQuestionsAsync(form.Id, cancellationToken);
            return View(form);
        }

        await _quizService.UpdateAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Quiz updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int moduleId, int formationId, CancellationToken cancellationToken)
    {
        await _quizService.DeleteAsync(id, cancellationToken);

        TempData["ToastMessage"] = "Quiz deleted.";
        TempData["ToastType"] = "info";
        return moduleId > 0
            ? RedirectToAction("Edit", "Modules", new { id = moduleId })
            : RedirectToAction("Edit", "Formations", new { id = formationId });
    }

    [HttpGet]
    public IActionResult CreateQuestion(int quizId) => View(new QuestionFormDto { QuizId = quizId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(QuestionFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _questionValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _quizService.CreateQuestionAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Question added.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.QuizId });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int id, CancellationToken cancellationToken)
    {
        var form = await _quizService.GetQuestionForEditAsync(id, cancellationToken);
        return form is null ? NotFound() : View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(QuestionFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _questionValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _quizService.UpdateQuestionAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Question updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.QuizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id, int quizId, CancellationToken cancellationToken)
    {
        await _quizService.DeleteQuestionAsync(id, cancellationToken);

        TempData["ToastMessage"] = "Question removed.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Edit), new { id = quizId });
    }
}
