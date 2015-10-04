﻿(function () {
    angular
        .module('webBellwether')
        .controller('managementIntegrationGamesController', ['$scope', '$timeout', 'integrationGamesService', 'sharedService', function ($scope, $timeout, integrationGamesService, sharedService) {
            //pagination
            $scope.currentPage = 0;
            $scope.pageSize = 10;
            $scope.numberOfPages = function () {
                return Math.ceil($scope.integrationGames.length / $scope.pageSize);
            }
            $scope.selectedGame = '';
            $scope.setSelectGame = function (selected) {
                if (selected.id == $scope.selectedGame.id) {
                    $scope.selectedGame = '';
                } else {
                    $scope.selectedGame = selected;
                }
            };
            // ********************

            //integration games
            $scope.integrationGames = [];
            $scope.getIntegrationGamesWithLanguages = function (lang) {
                integrationGamesService.IntegrationGamesWithAvailableLanguages(lang).then(function (x) {
                    $scope.integrationGames = [];
                    $scope.integrationGames = x.data;
                    $scope.setGameTranslationAfterLanguageChange();                    
                });
            };

            $scope.setGameTranslationAfterLanguageChange = function () {
                if ($scope.selectedGame != '') {
                    $scope.integrationGames.forEach(function (o) {
                        if (o.id == $scope.selectedGame.id && o.language.id == $scope.selectedLanguage) {
                            $scope.selectedGame = o;
                            return;
                        }
                    });
                    if ($scope.selectedGame.language.id != $scope.selectedLanguage)
                        $scope.selectedGame = '';
                }
            }
            // ********************

            //edit integration games
            $scope.editGame = function (selectedGame) {
                integrationGamesService.PutIntegrationGame(selectedGame).then(function (x) {
                    //tutaj trzeba by jeszcze obsluzyc jakies informowanie usera 
                });
            };
            // ********************

            //delete integration games
            $scope.deleteDialog = function () {
                var dialog = $('#deleteDialog').data('dialog');
                dialog.open();
            }
            $scope.countGameTranslation = function (selectedGame) {
                var result = 0;
                selectedGame.gameTranslations.forEach(function (x) {
                    if (x.hasTranslation)
                        result++;
                })
                return result;
            }
            $scope.deleteGame = function (selectedGame, removeAllTranslation) {
                var dialog = $('#deleteDialog').data('dialog');
                dialog.close();
                if (removeAllTranslation)
                    selectedGame.temporarySeveralTranslationDelete = removeAllTranslation;
                integrationGamesService.DeleteIntegrationGame(selectedGame).then(function (x) {
                    $scope.getIntegrationGamesWithLanguages($scope.selectedLanguage);
                    //after delete i must refresh all list 
                });


                


            };
            // ********************

            //game features 
            $scope.gameFeatures = [];
            $scope.getGameFeatuesModelWithDetails = function (lang) {
                integrationGamesService.GameFeatuesModelWithDetails(lang).then(function (x) {
                    $scope.gameFeatures = x.data;
                });
            };
            // ********************

            //base init
            $scope.getIntegrationGamesWithLanguages($scope.selectedLanguage);
            $scope.getGameFeatuesModelWithDetails($scope.selectedLanguage);
            // ********************

            //when language change 
            $scope.$on('languageChange', function () {
                $scope.getIntegrationGamesWithLanguages(sharedService.sharedmessage);
                $scope.getGameFeatuesModelWithDetails(sharedService.sharedmessage);              
            });
            // ********************


        }]);
})();
