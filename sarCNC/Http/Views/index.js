var app = angular.module('testApp', ['ngFileUpload', 'ui.bootstrap']);

app.controller("WebSocketController", function($scope, $window) {
  $scope.log = {
    messages: [ "init" ]
  };

  log = function(message)
  {
    $scope.log.messages.push(message);
  };

  $scope.focus = function(focused) {
    $scope.hotkeys = !focused
  };

  $scope.hotkeys = true;
  $scope.setup = {
    jogDistance : '1'
  };

  $scope.gcode = "G90\n"
  $scope.gcode += "G1 Z-20 F100\n"
  $scope.gcode += "G1 X-50 F1500\n"
  $scope.gcode += "G1 X0 Y50\n"
  $scope.gcode += "G1 X50 Y0\n"
  $scope.gcode += "G1 X0 Y-50\n"
  $scope.gcode += "G1 X-50 Y0\n"
  $scope.gcode += "G1 Y25\n"
  $scope.gcode += "G1 X-75\n"
  $scope.gcode += "G1 Z0\n"
  $scope.gcode += "G1 X0 Y0";
  $scope.distance = 10;
  $scope.machine = {
    status : {},
    x : 0,
    y : 0,
    z : 0
  }

  $scope.work = {
    x : 0,
    y : 0,
    z : 0
  }

  $scope.commands = [];

  $scope.grbl = function(command)
  {
    ws.send(command);
  }

  $scope.reset = function()
  {
    ws.send(String.fromCharCode(24));
  }

  $scope.keyPress = function(eve){
    if (eve.which === 16) {
        $rootScope.shiftOn = true;
    }
 }

 // key press
  angular.element($window).on('keydown', function(e) {
    if ($scope.hotkeys)
    {
      switch (e.keyCode) {
        case 87:
          $scope.grbl("G91\nG0 Y" + $scope.setup.jogDistance);
          break;
        case 83:
          $scope.grbl("G91\nG0 Y-" + $scope.setup.jogDistance);
          break;
        case 65:
          $scope.grbl("G91\nG0 X-" + $scope.setup.jogDistance);
          break;
        case 68:
          $scope.grbl("G91\nG0 X" + $scope.setup.jogDistance);
          break;
        case 81:
          $scope.grbl("G91\nG0 Z" + $scope.setup.jogDistance);
          break;
        case 69:
          $scope.grbl("G91\nG0 Z-" + $scope.setup.jogDistance);
          break;
        default:
          console.log(e.keyCode.toString());
      }
    }

    // estop
    if (e.keyCode == 27) {
      $scope.grbl("!");
    }
 });


  var ws = new ReconnectingWebSocket("ws://" + window.location.host + "/Grbl", null, {debug: false, reconnectInterval: 100});
  ws.onopen = function() {
    $scope.$apply();
  };

  ws.onmessage = function (evt)  {
    // sarCNC JSON status
    if (evt.data.startsWith("{") && evt.data.endsWith("}"))
    {
      var data = JSON.parse(evt.data);
      if (data.status) {
        $scope.machine = data;
      }
    }

    $scope.$apply();
  };

  ws.onclose = function() {
    log("websocket is closed")
    $scope.$apply();
  };
});
