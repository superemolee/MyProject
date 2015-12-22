
/// <summary>
/// Basic class that represents a phoneme for mary tts.
/// </summary>
public class Phoneme
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Phoneme"/> class.
	/// </summary>
    public Phoneme()
    {
        Start = 0;
        Duration = 0;
        IsVowel = false;
        IsSyllabic = false;
        IsVoiced = false;
        IsSonorant = false;
        IsPlosive = false;
        IsPause = false;
        IsNasal = false;
        IsLiquid = false;
        IsGlide = false;
        Sonority = 0;

    }
	/// <summary>
	/// The name of the phoneme.
	/// </summary>
    string m_name;
	
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
    public string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }
	
	/// <summary>
	/// The start time of the phoneme.
	/// </summary>
    int m_start;
	
	/// <summary>
	/// Gets or sets the start time.
	/// </summary>
	/// <value>
	/// The start.
	/// </value>
    public int Start
    {
        get { return m_start; }
        set { m_start = value; }
    }
	
	/// <summary>
	/// The duration of the phoneme.
	/// </summary>
    int m_duration;

	/// <summary>
	/// Gets or sets the duration.
	/// </summary>
	/// <value>
	/// The duration.
	/// </value>
    public int Duration
    {
        get { return m_duration; }
        set { m_duration = value; }
    }
    bool m_isVowel;

    public bool IsVowel
    {
        get { return m_isVowel; }
        set { m_isVowel = value; }
    }
	
	/// <summary>
	/// The is syllabic.
	/// </summary>
    bool m_isSyllabic;
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance is syllabic.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is syllabic; otherwise, <c>false</c>.
	/// </value>
    public bool IsSyllabic
    {
        get { return m_isSyllabic; }
        set { m_isSyllabic = value; }
    }
	
	/// <summary>
	/// The is voiced.
	/// </summary>
    bool m_isVoiced;
	/// <summary>
	/// Gets or sets a value indicating whether this instance is voiced.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is voiced; otherwise, <c>false</c>.
	/// </value>
    public bool IsVoiced
    {
        get { return m_isVoiced; }
        set { m_isVoiced = value; }
    }
	
	/// <summary>
	/// The is sonorant.
	/// </summary>
    bool m_isSonorant;
	/// <summary>
	/// Gets or sets a value indicating whether this instance is sonorant.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is sonorant; otherwise, <c>false</c>.
	/// </value>
    public bool IsSonorant
    {
        get { return m_isSonorant; }
        set { m_isSonorant = value; }
    }
	
	/// <summary>
	/// The is liquid.
	/// </summary>
    bool m_isLiquid;
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance is liquid.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is liquid; otherwise, <c>false</c>.
	/// </value>
    public bool IsLiquid
    {
        get { return m_isLiquid; }
        set { m_isLiquid = value; }
    }
	
	/// <summary>
	/// The is nasal.
	/// </summary>
    bool m_isNasal;
	/// <summary>
	/// Gets or sets a value indicating whether this instance is nasal.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is nasal; otherwise, <c>false</c>.
	/// </value>
    public bool IsNasal
    {
        get { return m_isNasal; }
        set { m_isNasal = value; }
    }
	
	/// <summary>
	/// The is glide.
	/// </summary>
    bool m_isGlide;
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance is glide.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is glide; otherwise, <c>false</c>.
	/// </value>
    public bool IsGlide
    {
        get { return m_isGlide; }
        set { m_isGlide = value; }
    }
	
	/// <summary>
	/// The is plosive.
	/// </summary>
    bool m_isPlosive;
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance is plosive.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is plosive; otherwise, <c>false</c>.
	/// </value>
    public bool IsPlosive
    {
        get { return m_isPlosive; }
        set { m_isPlosive = value; }
    }
	
	/// <summary>
	/// The is pause.
	/// </summary>
    bool m_isPause;
	/// <summary>
	/// Gets or sets a value indicating whether this instance is pause.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is pause; otherwise, <c>false</c>.
	/// </value>
    public bool IsPause
    {
        get { return m_isPause; }
        set { m_isPause = value; }
    }
	
	/// <summary>
	/// The sonority.
	/// </summary>
    int m_sonority;
	/// <summary>
	/// Gets or sets the sonority.
	/// </summary>
	/// <value>
	/// The sonority.
	/// </value>
    public int Sonority
    {
        get { return m_sonority; }
        set { m_sonority = value; }
    }
	
	/// <summary>
	/// The example.
	/// </summary>
    string m_example;
	
	/// <summary>
	/// Gets or sets the example.
	/// </summary>
	/// <value>
	/// The example.
	/// </value>
    public string Example
    {
        get { return m_example; }
        set { m_example = value; }
    }
}

