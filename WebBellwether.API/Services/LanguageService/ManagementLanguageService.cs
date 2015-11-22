﻿using System;
using System.Collections.Generic;
using System.Linq;
using WebBellwether.API.Entities.Translations;
using WebBellwether.API.Results;
using WebBellwether.API.Models.Translation;
using System.IO;
using Newtonsoft.Json;
using WebBellwether.API.Repositories.Abstract;
using Newtonsoft.Json.Linq;

namespace WebBellwether.API.Services.LanguageService
{
    public class ManagementLanguageService
    {
        private readonly IAggregateRepositories _repository;
        private readonly string _destinationPlace;
        public ManagementLanguageService(IAggregateRepositories repository, string destPlace)
        {
            _repository = repository;
            _destinationPlace = destPlace;
        }
        public List<Language> GetLanguages(bool getAll = false)
        {
            return getAll? _repository.LanguageRepository.Get().ToList(): _repository.LanguageRepository.GetWithInclude(x => x.IsPublic).ToList();
        }
        private Language GetLanguageByName(string languageName)
        {
            return _repository.LanguageRepository.GetWithInclude(x => x.LanguageName.Equals(languageName)).FirstOrDefault();
        }
        private Language GetLanguageById(int languageId)
        {
            return _repository.LanguageRepository.GetWithInclude(x => x.Id == languageId).FirstOrDefault();
        }
        public ResultStateContainer PostLanguage(Language language)
        {
            try
            {
                //check is language exists 
                Language entity = GetLanguageByName(language.LanguageName);
                if (entity != null)
                    return new ResultStateContainer { ResultState = ResultState.Failure , ResultMessage=ResultMessage.LanguageExists };
                //insert and return new id 
                entity = new Language { IsPublic = false, LanguageName = language.LanguageName, LanguageFlag = language.LanguageFlag, LanguageShortName = language.LanguageShortName };
                _repository.LanguageRepository.Insert(entity);
                _repository.Save();
                entity = GetLanguageByName(language.LanguageName);
                if(entity == null)
                    return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage=ResultMessage.LanguageExists };

                //create language file if error delete language row in db and return error
                ResultStateContainer createFileLanguageResult = CreateLanguageFile(entity.Id);
                if (createFileLanguageResult.ResultState != ResultState.Success)
                {
                    //delete language in db 
                    _repository.LanguageRepository.Delete(entity);
                    return createFileLanguageResult;
                }                     
                //if lang inserted must return lang id ...
                return new ResultStateContainer { ResultState = ResultState.Success ,ResultMessage= ResultMessage.LanguageAdded, ResultValue = entity };
            }
            catch (Exception)
            {
                return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage = ResultMessage.Error };
            }
        }

        public ResultStateContainer FillLanguageFile(IEnumerable<string> languageValues, int langaugeId)
        {
            try
            {
                string languagefileLocation = $"{_destinationPlace}{langaugeId}{".json"}";
                var json = File.ReadAllText(languagefileLocation);
                Dictionary<string,string> languageFile = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
                var enumerable = languageValues as string[] ?? languageValues.ToArray();
                if (enumerable.Count() != languageFile.Count())
                    return new ResultStateContainer {ResultState = ResultState.Failure ,ResultValue = ResultMessage.ContentLanguageFilesAreNotCompatible};
                for (int i = 0; i < languageFile.Count; i++)
                {
                    languageFile[languageFile.ElementAt(i).Key] = enumerable.ElementAt(i);
                }

                var output = JsonConvert.SerializeObject(languageFile, Formatting.Indented);
                File.WriteAllText(languagefileLocation, output);
                return new ResultStateContainer { ResultState = ResultState.Success, ResultMessage = ResultMessage.KeyValuesAreCorrectlyFilled };

            }
            catch (Exception e)
            {
                
                return  new ResultStateContainer {ResultState = ResultState.Failure , ResultValue = e};
            }
        }

        private Dictionary<string, string> GetLanguageFileKeys()
        {
            Language templateLanguage = _repository.LanguageRepository.GetWithInclude(x => x.IsPublic).FirstOrDefault();
            if (templateLanguage == null)
                return null;
            string templateFileLocation = _destinationPlace + templateLanguage.Id + ".json";
            string templateJson = File.ReadAllText(templateFileLocation);
            dynamic jsonTemplateObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(templateJson);
            if (jsonTemplateObj == null)
                return null;
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in jsonTemplateObj)
            {
                result.Add(item.Key, "");
            }
            return result;
        }

        public IEnumerable<string> GetLanguageFileValue(int languageId)
        {
            Language language = _repository.LanguageRepository.GetWithInclude(x => x.Id == languageId).FirstOrDefault();
            if (language == null)
                return null;
            string languageFileLocation = $"{_destinationPlace}{language.Id}{".json"}";
            string templateJson = File.ReadAllText(languageFileLocation);
            dynamic valuesFromFile = JsonConvert.DeserializeObject<Dictionary<string, string>>(templateJson);
            if (valuesFromFile == null)
                return null;
            List<string> result = new List<string>();
            foreach (var item in valuesFromFile)
            {
                result.Add(item.Value);
            }
            return result.AsEnumerable();
        } 

        public ResultStateContainer CreateLanguageFile(int newLanguageId)
        {
            try
            {
                if (newLanguageId == 0)
                    return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage=ResultMessage.LanguageNotExists };
                //take key for new file                
                Dictionary<string, string> languageFileKeys = GetLanguageFileKeys();
                if (languageFileKeys == null)
                    return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage=ResultMessage.LanguageFileNotExists };

                string newFileLocation = _destinationPlace + newLanguageId + ".json";
                
                //create json object
                JObject jsonKeys = new JObject();
                foreach (var item in languageFileKeys)
                {
                    jsonKeys.Add(new JProperty(item.Key, item.Value));
                }

                //create new file
                using (StreamWriter file = File.CreateText(newFileLocation))

                //save keys to file
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    jsonKeys.WriteTo(writer);
                }               
                    return new ResultStateContainer { ResultState = ResultState.Success,ResultMessage=ResultMessage.LanguageFileAdded };
            }
            catch (Exception)
            {
                return new ResultStateContainer { ResultState = ResultState.Failure, ResultMessage=ResultMessage.Error };
            }
        }
        public ResultStateContainer PutLanguageKey(LanguageKeyModel languageKey)
        {
            try
            {
                string jsonLocation = _destinationPlace + languageKey.LanguageId + ".json";
                string json = File.ReadAllText(jsonLocation);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                jsonObj[languageKey.Key] = languageKey.Value;
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(jsonLocation, output);
                return new ResultStateContainer { ResultState = ResultState.Success,ResultMessage= ResultMessage.LanguageKeyValueEdited };
            }
            catch (Exception)
            {

                return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage = ResultMessage.Error };
            }
        }
        public ResultStateContainer PublishLanguage(Language language)
        {
            try
            {
                int emptyKey = 0;
                if (language.IsPublic) //public language
                {
                    string jsonLocation = _destinationPlace + language.Id + ".json";
                    string json = File.ReadAllText(jsonLocation);
                    dynamic jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (jsonObj == null)
                        return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage=ResultMessage.LanguageFileNotExists };
                    foreach (var item in jsonObj)
                    {
                        if (item.Value.Length < 2)
                            emptyKey++;
                    }
                    if (emptyKey > 0)
                        return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage=ResultMessage.EmptyKeysExists };
                }
                else //nonpublic language
                {
                    if (_repository.LanguageRepository.GetWithInclude(x => x.IsPublic).Count() == 1)  //check how many languages is public 
                        return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage=ResultMessage.OnlyOnePublicLanguage };
                }
                Language entity = _repository.LanguageRepository.GetWithInclude(x => x.Id == language.Id).FirstOrDefault();
                if (entity == null)
                    return new ResultStateContainer { ResultState = ResultState.Failure, ResultMessage =ResultMessage.LanguageNotExists };
                entity.IsPublic = language.IsPublic;
                _repository.Save();
                return language.IsPublic ? new ResultStateContainer { ResultState = ResultState.Success,ResultMessage=ResultMessage.LanguageHasBeenPublished } : new ResultStateContainer { ResultState = ResultState.Success,ResultMessage = ResultMessage.LanguageHasBeenNonpublic };
            }
            catch (Exception)
            {

                return new ResultStateContainer { ResultState = ResultState.Failure, ResultMessage = ResultMessage.Error};
            }
        }

        public ResultStateContainer PutLanguage(Language language)
        {
            try
            {
                Language entity = _repository.LanguageRepository.GetWithInclude(x => x.Id == language.Id).FirstOrDefault();
                if (entity == null)
                    return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage = ResultMessage.LanguageNotExists };
                entity.LanguageFlag = language.LanguageFlag;
                entity.LanguageName = language.LanguageName;
                entity.LanguageShortName = language.LanguageShortName;
                _repository.Save();
                return new ResultStateContainer { ResultState = ResultState.Success , ResultMessage = ResultMessage.LanguageEdited };
            }
            catch (Exception)
            {

                return new ResultStateContainer { ResultState = ResultState.Failure, ResultMessage = ResultMessage.Error };
            }
        }
        private ResultStateContainer DeleteLanguageFile(int languageId)
        {
            try
            {
                string file = _destinationPlace + languageId + ".json";
                if(File.Exists(file))
                {
                    File.Delete(file);
                    return new ResultStateContainer { ResultState = ResultState.Success , ResultMessage = ResultMessage.LanguageFileRemoved };
                }
                return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage = ResultMessage.LanguageFileNotExists };
            }
            catch (Exception)
            {

                return new ResultStateContainer { ResultState = ResultState.Failure, ResultMessage = ResultMessage.Error };
            }
        }
        public ResultStateContainer DeleteLanguage(Language language)
        {
            try
            {
                //BETA VERSION NOT REMOVE OTHER STRUCTURE ... 
                if (language.Id == 1 | language.Id == 2) // i need list of irremovable languages ... 
                    return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage = ResultMessage.LanguageCanNotBeRemoved };
                Language entity = GetLanguageById(language.Id);
                if (entity == null)
                    return new ResultStateContainer { ResultState = ResultState.Failure,ResultMessage = ResultMessage.LanguageNotExists };
                _repository.LanguageRepository.Delete(entity);

                ResultStateContainer deleteFileResult = DeleteLanguageFile(language.Id);
                if (deleteFileResult.ResultState != ResultState.Success)
                    return deleteFileResult;
                _repository.Save();
                return new ResultStateContainer { ResultState = ResultState.Success,ResultMessage = ResultMessage.LanguageRemoved };
            }
            catch (Exception)
            {
                return new ResultStateContainer { ResultState = ResultState.Failure, ResultMessage = ResultMessage.Error };
            }
        }
    }
}
