﻿using System.Web.Http;
using System.Web.Http.Results;
using WebBellwether.API.Utility;
using WebBellwether.Models.ViewModels;
using WebBellwether.Models.ViewModels.Joke;

namespace WebBellwether.API.Controllers
{
    [RoutePrefix("api/JokeCategoryManagement")]
    public class JokeCategoryManagementController : ApiController
    {
        [Authorize(Roles = "Admin")]
        [Route("PostEditJokeCategory")]

        public JsonResult<ResponseViewModel<bool>> PostEditJokeCategory(JokeCategoryViewModel jokeCategory)
        {
            var response = ServiceExecutor.Execute(() => ServiceFactory.JokeCategoryManagementService.PutJokeCategory(jokeCategory));
            return Json(response);
        }

        [Authorize(Roles = "Admin")]
        [Route("PostJokeCategory")]
        public JsonResult<ResponseViewModel<JokeCategoryViewModel>> PostJokeCategory(JokeCategoryViewModel jokeCategory)
        {
            var response = ServiceExecutor.Execute(() => ServiceFactory.JokeCategoryManagementService.InsertJokeCategory(jokeCategory));
            return Json(response);
        }

        [AllowAnonymous]
        [Route("GetJokeCategories")]
        public JsonResult<ResponseViewModel<JokeCategoryViewModel[]>> GetJokeCategories(int language)
        {
            var response = ServiceExecutor.Execute(() => ServiceFactory.JokeCategoryManagementService.GetJokeCategories(language));
            return Json(response);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetJokeCategoryTranslation")]
        public JsonResult<ResponseViewModel<JokeCategoryViewModel>> GetJokeCategoryTranslation(int id, int languageId)
        {
            var response =
                ServiceExecutor.Execute(() => ServiceFactory.JokeCategoryManagementService.GetJokeCategoryTranslation(id, languageId));
            return Json(response);
        }

        [Authorize(Roles = "Admin")]
        [Route("PostDeleteJokeCategory")]
        public JsonResult<ResponseViewModel<bool>> PostDeleteJokeCategory(JokeCategoryViewModel jokeCategory)
        {
            var response = ServiceExecutor.Execute(() => ServiceFactory.JokeCategoryManagementService.RemoveJokeCategory(jokeCategory));
            return Json(response);
        }
    }
}