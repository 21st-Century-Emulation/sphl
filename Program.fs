module SPHL.App

open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

[<CLIMutable>]
type CpuFlags =
    {
        Sign : bool
        Zero : bool
        AuxCarry : bool
        Parity : bool
        Carry : bool
    }

[<CLIMutable>]
type CpuState =
    {
        A : byte
        B : byte
        C : byte
        D : byte
        E : byte
        H : byte
        L : byte
        StackPointer : uint16
        ProgramCounter : uint16
        Cycles : uint64
        Flags : CpuFlags
        InterruptsEnabled : bool
    }

[<CLIMutable>]
type Cpu = 
    {
        Opcode : byte
        Id : string
        State : CpuState
    }

// ---------------------------------
// Web app
// ---------------------------------
let sphlHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! cpu = ctx.BindJsonAsync<Cpu>()
            let highByte = (uint16 cpu.State.H) <<< 8
            let updatedStackPointer = (uint16 cpu.State.L) ||| highByte

            let updatedState = { cpu.State with StackPointer = updatedStackPointer; Cycles = cpu.State.Cycles + uint64 5 }
            let updatedCpu = { cpu with State = updatedState }

            return! json updatedCpu next ctx
        }

let webApp =
    choose [
        POST >=> route "/api/v1/execute" >=> sphlHandler
        GET  >=> route "/status" >=> text "Healthy"
    ]

type Startup() =
    member __.ConfigureServices (services : IServiceCollection) =
        services.AddGiraffe() |> ignore

    member __.Configure (app : IApplicationBuilder)
                        (_ : IHostEnvironment)
                        (_ : ILoggerFactory) =
        app.UseGiraffe webApp

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseStartup<Startup>()
                    |> ignore)
        .Build()
        .Run()
    0