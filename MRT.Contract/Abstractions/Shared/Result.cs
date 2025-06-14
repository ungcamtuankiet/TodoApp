﻿namespace MRT.Contracts.Abstractions.Shared;

public class Result<T>
{
    public int? Error { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
