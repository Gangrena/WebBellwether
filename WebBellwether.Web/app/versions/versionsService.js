﻿(function () {
    angular
        .module('webBellwether')
        .factory('versionsService', ['$http', '$resource', 'ngAuthSettings', function ($http, $resource, ngAuthSettings) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            var getVersionDetailsForLanguage = function (language) {
                return $http.get(serviceBase + 'api/Version/GetVersionDetailsForLanguage/?languageId=' + language).then(function (x) {
                    return x;
                });
            };
            var postNewVersion = function(newVersionModel) {
                return $http.post(serviceBase + 'api/Version/PostNewVersion/', newVersionModel).then(function(x) {
                    return x;
                });
            };
            var postRemoveVersion = function(versionForRemove) {
                return $http.post(serviceBase + 'api/Version/PostRemoveVersion/', versionForRemove).then(function(x) {
                    return x;
                });
            };

            var managementVersionsServiceFactory = {};
            managementVersionsServiceFactory.GetVersionDetailsForLanguage = getVersionDetailsForLanguage;
            managementVersionsServiceFactory.PostNewVersion = postNewVersion;
            managementVersionsServiceFactory.PostRemoveVersion = postRemoveVersion;
            return managementVersionsServiceFactory;
        }]);
})();
