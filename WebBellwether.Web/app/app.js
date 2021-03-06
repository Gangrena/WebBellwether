﻿(function () {
    angular.module('webBellwether', ['ngRoute', 'LocalStorageModule', 'ngResource', 'angular-loading-bar', 'ngAnimate', 'customFilters', 'customAnimations','ui.bootstrap']);
})();


(function () {
    angular
        .module('webBellwether')
        .constant('ngAuthSettings', {
            //*************************
            apiServiceBaseUri: '',
            //*************************
            clientId: 'ngAuthApp'

        });
})();


(function () {
    angular
        .module('webBellwether')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('authInterceptorService');
        });
})();

(function () {
    angular
        .module('webBellwether')
        .run(['authService', function (authService) {
            authService.fillAuthData();
        }]);
})();
