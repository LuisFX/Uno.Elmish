﻿[<RequireQualifiedAccess>]
module Elmish.WPF.Utilities.ViewModel

open System.Windows
open Elmish
open Elmish.WPF
open Elmish.WPF.Internal
open Windows.UI.Xaml
open Windows.UI.Core

/// Start Elmish dispatch loop
let internal startLoop
    (config: ElmConfig)
    (element: FrameworkElement)
    (programRun: Program<'t, 'model, 'msg, BindingSpec<'model, 'msg> list> -> unit)
    (program: Program<'t, 'model, 'msg, BindingSpec<'model, 'msg> list>) =
  let mutable lastModel = None

  let setState model dispatch =
    match lastModel with
    | None ->
        let mapping = program.view model dispatch
        let vm = ViewModel<'model,'msg>(model, dispatch, mapping, config)
        element.DataContext <- box vm
        lastModel <- Some vm
    | Some vm ->
        vm.UpdateModel model

  let uiDispatch (innerDispatch: Dispatch<'msg>) : Dispatch<'msg> =
    fun msg -> element.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, fun () -> innerDispatch msg) |> ignore

  programRun { program with setState = setState; syncDispatch = uiDispatch }


/// Creates a design-time view model using the given model and bindings.
let designInstance (model: 'model) (bindings: BindingSpec<'model, 'msg> list) =
  ViewModel(model, ignore, bindings, ElmConfig.Default)
