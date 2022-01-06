# hue - a tool to control your Philips Hue connected devices
This little tool allows you to connect to a Hue Bridge on your network and control lights connected to it from either the command line or a connected midi device.

There are a number of out-of-the-box actions you can execute once connected and paired:
 - Turn light(s) on or off
 - Set the brightness of light(s)
 - Assign random colours to all light(s)
 - Apply a scene (that is stored on the bridge!)
 - Store the current state of light(s) as a 'profile'
 - Apply a previously saved profile
 - Map multiple actions to incoming control events (like pressing a button on your controller)
 - Adding new actions is easy - just extend `HueActionBase`

## Connecting to your Bridge
 1. Run `hue connect <ip>` to pair with a specific Bridge (eg. `hue connect https://192.168.1.140`)
 1. Run `hue select <name>` to select a particular `room` or `zone` (eg. `hue select room lounge`) as the default target for any actions
 1. You're ready to go! You can use `hue status` to check everything is configured correctly

## Run actions from the command line
An `action` is something that changes the state of either the Hue tool or your Hue Bridge by sending one or more commands. Actions will affect the target you `select`ed earlier by default. Some actions take parameters (eg. colour or brightness level).

### See available actions
You can get a list of available actions by running `hue action list`. This will list out all available actions, any aliases, and details on any parameters they accept.

### Run a simple action
Turn on your lights with `hue action turnon`. This will turn on the default control target you set above by finding the first `group light` resource it can.

### Run an action with parameters
Set the brightness of your target with `hue action dim level=50`. This will set the brightness level of all `light` resources it finds in the default control target

### Run an action against a specific target
If you want to specify a particular target for an action (instead of the default), you can pass the target=name parameter to any action. For example, if you have a room called 'west drawing room', you can set the brightness with `hue action dim level=50 target="west drawing room"`. You can target either a `room`, `zone`, `group light` or individual `light` with this method (in that order of precedence), by name or id.

## Use a MIDI controller
**Windows/macOS only for now:** I'll be porting over to something a bit more cross platform at some point

### Test your MIDI controller
You can check that `hue` can see your MIDI controller by running `hue log-input`. This will log any any control messages it receives from any connected and accessible MIDI devices.
We support two types of input: Button (note) and Scalar (control). A button can register a `ShortPress` - when the button is pressed and released quickly, and a `LongPress`, where the button is held for > 1 second. A Scalar input carries a value. As you press notes and move controls on your MIDI controller, you should see log messages showing `hue` capturing these *control events*.

### Map MIDI controls to actions
You map *control events* to actions via *routes*. A Route is a rule which decides whether a given control event should fire a particular action. You can manually create routes in your config file, but the `map` command exists to allow you to quickly set up simple routes.

To map a button on your MIDI controller to set the brightness, run `hue map dim level=50`. You will then be prompted to provide a control event to map this action to. Press a button or twiddle a control knob on your controller, and the route will be saved.

You can save multiple actions against one control event - when the control event is run, all applicable routes will be run, so you can have one button perform multiple actions.

If you try to map a scalar input to a 'button style' action (eg. mapping a control knob to 'turn on'), the action will execute when the value is greater than 25%. See 'routes' for details on how to customise this.

### Clear all actions for a given button
If you want to clear everything configured against a given input, run `hue map clear` and provide a control event. This will remove all routes associated with that button.

## Routes
A `route` is a set of rules used to filter incoming control events, and an action to perform if an event matches. Multiple routes can be fired for a single control event if they match.
See a list of all configured routes with `hue route list`. Delete routes with `hue route del <index>` where `<index>` is the number from the 'ix' column of `list`.

You can clear all routes with `hue route clear`.

You can test a route with `hue route test <index>` - keep in mind you'll have no control event, so values may not be present in the action run.

### Rules
Routes are matched based on:
 - `inputId` (required) - the unique ID of the input which fired the event
 - `eventType` (optional) - either `scalarChanged`, `shortPress` or `longPress`
 - `triggerAbove` (optional) - when a `scalarValue` event is checked, fire only if the value is greater than or equal to this value
 - `triggerBelow` (optional) - when a `scalarValue` event is checked, fire only if the value is less than this value
