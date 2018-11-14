# GazeTools
_Unity (C-Sharp) classes for VR user interactions_

## Installation
Add the contents of this repository to your Unity Project's Assets folder

## Documentation
Doxygen html documentation: https://fusefactory.github.io/GazeTools/docs/html/annotated.html

## Usage: Gazeable
```Gazeable``` is basically a fancy semantic wrapper around a single boolean state, which is true when the subject is "Gazed At" and false when not.

The ```Gazeable``` MonoBehaviour by itself doesn't do much, it just manages its boolean "IsGazedAt" state, and relies on other input components to update its state.


Therefore it provides several public methods, which can be used in UnityActions (which can be created
in the UnityEditor to be executed when a UnityEvent is invoked);

#### Input: starting/ending Gazer instances
A Gazeable can have multiple simultanous "Gazer" instances. Whenever it has at least one _active_ Gazer instance, its boolean IsGazedAt state will be true, otherwise it will be false.

```c
// Start/End gazeable-managed gazer instances
void StartGazer(Object)
void EndGazer(Object)
void ToggleGazer(Object)

// Start/End caller-managed gazer instances
Gazer StartGazer() // Returns an activated Gazer instance
Gazer GetGazer() // Returns an deactivated Gazer instance
// To END a gazer, the caller should call the Deactivate of Dispose method on the Gazer instance
```

Included input components that use the above API to update the Gazeable's status;
 - ```GazeRayInput``` - requires a Camera and a UnityEngine.UI.Graphic and uses Raycasting to detect a hit
 - ```GazeColliderInput``` - requires a Camera and a Collider and uses Raycasting to detect a hit
 - ```GazeFocusInput``` - requires two transform instances and compares the angle between the observing Transform's forward vector and the vector from the observer's position to the observable's position against a configurable maximum angle value.

#### Output: gaze start/end events
Various events are invoked when the IsGazedAt state of the Gazeable changes:

```c
UnityEvent OnGazeStart;
UnityEvent OnGazeEnd;
UnityEvent<Gazeable> GazeChangeEvent;
UnityEvent<Gazeable> GazeStartEvent;
UnityEvent<Gazeable> GazeEndEvent;
```

## Usage: Chargeable
TODO
