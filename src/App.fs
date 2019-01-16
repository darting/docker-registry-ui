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
open Fetch.Fetch_types
open Thoth.Json
open Types

// configures
let endpoint = "https://registry.uat/v2"

// Api
let getCatelog () =
    promise {
        let url = sprintf "%s/_catalog" endpoint
        let! res = Fetch.fetch url []
        let! txt = res.text()
        return Decode.Auto.unsafeFromString<Catelog> txt
    }

let getRepositoryTags (repo : string) =
    promise {
        let url = sprintf "%s/%s/tags/list" endpoint (encodeURIComponent repo)
        let! res = Fetch.fetch url []
        let! txt = res.text()
        return Decode.Auto.unsafeFromString<RepositoryTags> txt
    }

let getManifest (name : string) (reference : string) =
  promise {
    let url = sprintf "%s/%s/manifests/%s" endpoint (encodeURIComponent name) reference
    let props = [ RequestProperties.Method HttpMethod.GET ]
    let! res = Fetch.fetch url props
    let digest = res.Headers.get "Docker-Content-Digest"
    let! txt = res.text()
    let manifest = Decode.Auto.unsafeFromString<Manifest> txt
    return { manifest with digest = digest }
  }

let getImageDigest (name : string) (reference : string) =
  promise {
    let url = sprintf "%s/%s/manifests/%s" endpoint (encodeURIComponent name) reference
    let props = [ RequestProperties.Method HttpMethod.HEAD ]
    let! res = Fetch.fetch url props
    let x = res.Headers.get "Docker-Content-Digest"
    console.log("digest",x,res.Headers)
    return x
  }

let deleteImage (name : string) (reference : string) =
  promise {
    let url = sprintf "%s/%s/manifests/%s" endpoint (encodeURIComponent name) reference
    let props = [ RequestProperties.Method HttpMethod.DELETE ]
    let! res = Fetch.fetch url props
    let! txt = res.text()
    return txt
  }

let init() =
    { Repositories = [] },
    Cmd.ofPromise getCatelog () CatelogFetched FetchError

// UPDATE
let update (msg : Msg) (model : Model) =
    match msg with
    | DeleteWithTag (repo, tag) ->
      model, Cmd.ofPromise (getImageDigest repo) tag ImageDigestFetched FetchError
    | CatelogFetched catelog ->
        let repositories =
            catelog.repositories
            |> List.map (fun x ->
                   { Name = x
                     Tags = [] })
            |> List.sortBy (fun x -> x.Name)
        let cmd = 
          catelog.repositories
          |> List.map (fun x -> Cmd.ofPromise getRepositoryTags x RepositoryTagsFetched FetchError)
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
    | ImageDigestFetched digest ->
      console.log(digest)
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


// views
let viewTag (repo : string) (tag : string) dispatch = 
  div [ Styles.tag ] 
      [ span [] [ str tag ]
        button [ OnClick (fun _ -> dispatch (DeleteWithTag (repo, tag)) ) ] [ str "X" ] ]

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
