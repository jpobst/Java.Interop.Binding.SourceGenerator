package com.example;

public class Mathinator
{
  int _version = 1;
  
  public final int add (int a, int b) { return a + b; }
  
  public final void setVersion (int version) { _version = version; }
  
  public final int getVersion () { return _version; }

}