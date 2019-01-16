module Api

open Fable.SimpleHttp
open Thoth.Json
open Types

let getCatelog (endpoint : string) =
    async {
        let url = sprintf "%s/_catalog" endpoint
        let! (code, rsp) = Http.get url
        return Decode.Auto.unsafeFromString<Catelog> rsp
    }

let getRepositoryTags (endpoint : string) (repo : string) =
    async {
        let url = sprintf "%s/%s/tags/list" endpoint (Fable.Import.JS.encodeURIComponent repo)
        let! (code, rsp) = Http.get  url
        return Decode.Auto.unsafeFromString<RepositoryTags> rsp
    }

let getImageDigest (endpoint : string) (name : string) (reference : string) =
  async {
    let url = sprintf "%s/%s/manifests/%s" endpoint (Fable.Import.JS.encodeURIComponent name) reference
    let! rsp = 
        Http.request url
        |> Http.method HttpMethod.HEAD
        |> Http.send
    Fable.Import.JS.console.log("headers", rsp.responseHeaders)
    let x = rsp.responseHeaders
            |> Map.tryFind "Docker-Content-Digest"
    return x
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

// let deleteImage (name : string) (reference : string) =
//   promise {
//     let url = sprintf "%s/%s/manifests/%s" endpoint (encodeURIComponent name) reference
//     let props = [ RequestProperties.Method HttpMethod.DELETE ]
//     let! res = Fetch.fetch url props
//     let! txt = res.text()
//     return txt
//   }
