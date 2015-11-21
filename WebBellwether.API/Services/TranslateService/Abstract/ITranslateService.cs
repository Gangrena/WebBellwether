﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBellwether.API.Models.Translation;
using WebBellwether.API.Results;

namespace WebBellwether.API.Services.TranslateService.Abstract
{
    public interface ITranslateService
    {
        List<SupportedLanguage> GetListOfSupportedLanguages();
        string GetServiceName();
        Task<ResultStateContainer> GetLanguageKeyTranslation(string currentLanguage, string targetLanguage,string content);

        Task<ResultStateContainer> TranslateAllLanguageKeys(Dictionary<string, string> currentLanguageFile,
            string currentLanguage, string targetLanguage);
    }
}
