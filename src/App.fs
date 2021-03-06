module App

open System
open Elmish
open Elmish.React
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.JS
open Fable.Import.Browser
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Types

// configures
let endpoint = "https://registry.uat/v2"

let getCatelog () = Api.getCatelog endpoint

let getImageDigest = Api.getImageDigest endpoint

let getRepositoryTags = Api.getRepositoryTags endpoint

let deleteImage = Api.deleteImage endpoint

let init() =
    { Repositories = [] },
    Cmd.ofAsync getCatelog  () CatelogFetched FetchError

// UPDATE
let update (msg : Msg) (model : Model) =
    match msg with
    | DeleteImage (name, tag) ->
      model, Cmd.ofAsync (getImageDigest name) tag 
                         (function
                          | Some digest -> ImageDigestFetched (name, digest)
                          | None -> FetchError (exn "can't get image digest")) FetchError
    | DeleteLayer (name, tag) ->
      model, Cmd.none
    | CatelogFetched catelog ->
        let repositories =
            catelog.repositories
            |> List.map (fun x ->
                   { Name = x
                     Tags = [] })
            |> List.sortBy (fun x -> x.Name)
        let cmd = 
          catelog.repositories
          |> List.map (fun x -> Cmd.ofAsync getRepositoryTags x RepositoryTagsFetched FetchError)
          |> Cmd.batch
        { model with Repositories = repositories }, cmd
    | RepositoryTagsFetched repositoryTags ->
      match model.Repositories |> List.tryFind (fun x -> x.Name = repositoryTags.name) with
      | Some repo -> 
        let repo' = { repo with Tags = repositoryTags.tags }
        let rest = model.Repositories |> List.filter (fun x -> x.Name <> repo'.Name)
        { model with Repositories = repo' :: rest |> List.sortBy (fun x -> x.Name) }
        , Cmd.none
      | None -> 
        model, Cmd.none
    | ImageDigestFetched (name, digest) ->
      model, Cmd.ofAsync (deleteImage name) digest 
                         (function
                          | Ok _ -> ImageDeleted
                          | Error code -> code.ToString() |> exn |> FetchError) FetchError
    | ImageDeleted ->
      model, Cmd.none                          
    | FetchError ex -> model, Cmd.none

// styles
module Styles =
  let container = 
    Style [ Width "60rem"
            Margin "auto" ]

  let card =
    Style [ Border "0.05rem solid #ddd"
            Margin "0.25rem"
            Padding "1rem"
            BorderRadius "0.2rem" ]

  let title = 
    Style [ FontWeight "800"
            FontSize "1.5rem"
            MarginBottom "1rem"
            Display "block" ]

  let tag =
    Style [ Border "0.05rem solid #ddd"
            Margin "0.1rem"
            Padding "0.5rem"
            BorderRadius "0.25rem"
            Display "inline"
            TextAlign "center"
            VerticalAlign "baseline"
            LineHeight "1"
            WhiteSpace "nowrap" ]

  let m1 =     
    Style [ Margin "0.2rem" ]       


// views
let viewTag (repo : string) (tag : string) dispatch = 
  div [ Styles.tag ] 
      [ span [] [ str tag ]
        button [ Styles.m1
                 OnClick (fun _ -> dispatch (DeleteImage (repo, tag)) ) ] [ str "DEL IMAGE" ]
        button [ Styles.m1
                 OnClick (fun _ -> dispatch (DeleteLayer (repo, tag)) ) ] [ str "DEL LAYER" ] ]

let viewRepositorySummary (repo : Repository) dispatch =
    div [ Styles.card ] 
        [ yield div [ Styles.title ] [ str repo.Name ]
          yield! repo.Tags |> List.map (fun x -> viewTag repo.Name x dispatch) ]

let view (model : Model) dispatch =
    div [ Styles.container ] [ yield! model.Repositories |> List.map (fun x -> viewRepositorySummary x dispatch) ]

// App
Program.mkProgram init update view
|> Program.withReact "elmish-app"
|> Program.withConsoleTrace
|> Program.run
