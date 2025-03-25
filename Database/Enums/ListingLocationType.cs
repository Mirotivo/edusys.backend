using System;

[Flags]
public enum ListingLocationType
{
    Webcam = 1 << 1,            // 0010
    TutorLocation = 1 << 2,     // 0100
    StudentLocation = 1 << 3,   // 1000
}

