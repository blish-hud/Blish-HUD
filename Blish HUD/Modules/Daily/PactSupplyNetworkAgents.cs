
public class Rootobject {
    public Class1[] Property1 { get; set; }
}

public class Class1 {
    public int Day { get; set; }
    public Agent[] Agents { get; set; }
}

public class Agent {
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string Waypoint { get; set; }
}
