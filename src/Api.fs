module Api

open Fable.SimpleHttp
open Thoth.Json
open Types

let [<Literal>] manifestType = "application/vnd.docker.distribution.manifest.v2+json"

let safeUri = Fable.Import.JS.encodeURIComponent

let getCatelog (endpoint : string) =
    async {
        let url = sprintf "%s/_catalog" endpoint
        let! (code, rsp) = Http.get url
        return Decode.Auto.unsafeFromString<Catelog> rsp
    }

let getRepositoryTags (endpoint : string) (repo : string) =
    async {
        let url = sprintf "%s/%s/tags/list" endpoint (safeUri repo)
        let! (code, rsp) = Http.get  url
        return Decode.Auto.unsafeFromString<RepositoryTags> rsp
    }

let getImageDigest (endpoint : string) (name : string) (reference : string) =
  async {
    let url = sprintf "%s/%s/manifests/%s" endpoint (safeUri name) reference
    let! rsp = 
        Http.request url
        |> Http.method HttpMethod.HEAD
        |> Http.header (Headers.accept "application/vnd.docker.distribution.manifest.v2+json")
        |> Http.send
    return Map.tryFind "docker-content-digest" rsp.responseHeaders
  }

let deleteImage (endpoint : string) (name : string) (reference : string) =
  async {
    let url = sprintf "%s/%s/manifests/%s" endpoint (safeUri name) (safeUri reference)
    let! rsp =
        Http.request url
        |> Http.method HttpMethod.DELELE
        |> Http.send

    return match rsp.statusCode with
           | 202 -> Ok ()
           | _ -> Error rsp.statusCode
  }

let deleteLayer (endpoint : string) (name : string) (reference : string) =
  async {
    let url = sprintf "%s/%s/blobs/%s" endpoint (safeUri name) (safeUri reference)
    let! rsp =
        Http.request url
        |> Http.method HttpMethod.DELELE
        |> Http.send

    return match rsp.statusCode with
           | 202 -> Ok ()
           | _ -> Error rsp.statusCode
  }


// let getCatelog () =
//     promise {
//         let url = sprintf "%s/_catalog" endpoint
//         let! res = Fetch.fetch url []
//         let! txt = res.text()
//         return Decode.Auto.unsafeFromString<Catelog> txt
//     }

// let getRepositoryTags (repo : string) =
//     promise {
//         let url = sprintf "%s/%s/tags/list" endpoint (encodeURIComponent repo)
//         let! res = Fetch.fetch url []
//         let! txt = res.text()
//         return Decode.Auto.unsafeFromString<RepositoryTags> txt
//     }

// let getManifest (name : string) (reference : string) =
//   promise {
//     let url = sprintf "%s/%s/manifests/%s" endpoint (encodeURIComponent name) reference
//     let props = [ RequestProperties.Method HttpMethod.GET ]
//     let! res = Fetch.fetch url props
//     let digest = res.Headers.get "Docker-Content-Digest"
//     let! txt = res.text()
//     let manifest = Decode.Auto.unsafeFromString<Manifest> txt
//     return { manifest with digest = digest }
//   }

// let getImageDigest (name : string) (reference : string) =
//   promise {
//     let url = sprintf "%s/%s/manifests/%s" endpoint (encodeURIComponent name) reference
//     let props = [ RequestProperties.Method HttpMethod.HEAD ]
//     let! res = Fetch.fetch url props
//     let x = res.Headers.get "Docker-Content-Digest"
//     console.log("digest",x,res.Headers)
//     return x
//   }