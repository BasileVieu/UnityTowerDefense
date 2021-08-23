using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct EnemyAnimator
{
    public enum Clip
    {
        Move,
        Intro,
        Outro,
        Dying,
        Appear,
        Disappear
    }
    
    private PlayableGraph graph;

    private AnimationMixerPlayable mixer;

    private Clip previousClip;

    private float transitionProgress;
    private const float transitionSpeed = 5.0f;

    private bool hasAppearClip;
    private bool hasDisappearClip;
    
    #if UNITY_EDITOR
        public bool IsValid => graph.IsValid();
    #endif
    
    #if UNITY_EDITOR
        private double clipTime;
    #endif
    
    public Clip CurrentClip { get; private set; }

    public bool IsDone => GetPlayable(CurrentClip).IsDone();
    
    public void Configure(Animator _animator, EnemyAnimationConfig _config)
    {
        hasAppearClip = _config.Appear;
        hasDisappearClip = _config.Disappear;
        
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        mixer = AnimationMixerPlayable.Create(graph, hasAppearClip || hasDisappearClip ? 6 : 4);

        var clip = AnimationClipPlayable.Create(graph, _config.Move);
        clip.Pause();
        mixer.ConnectInput((int) Clip.Move, clip, 0);

        clip = AnimationClipPlayable.Create(graph, _config.Intro);
        clip.SetDuration(_config.Intro.length);
        mixer.ConnectInput((int) Clip.Intro, clip, 0);

        clip = AnimationClipPlayable.Create(graph, _config.Outro);
        clip.SetDuration(_config.Outro.length);
        clip.Pause();
        mixer.ConnectInput((int) Clip.Outro, clip, 0);

        clip = AnimationClipPlayable.Create(graph, _config.Dying);
        clip.SetDuration(_config.Dying.length);
        clip.Pause();
        mixer.ConnectInput((int) Clip.Dying, clip, 0);

        if (hasAppearClip)
        {
            clip = AnimationClipPlayable.Create(graph, _config.Appear);
            clip.SetDuration(_config.Appear.length);
            clip.Pause();

            mixer.ConnectInput((int) Clip.Appear, clip, 0);
        }

        if (hasDisappearClip)
        {
            clip = AnimationClipPlayable.Create(graph, _config.Disappear);
            clip.SetDuration(_config.Disappear.length);
            clip.Pause();

            mixer.ConnectInput((int) Clip.Disappear, clip, 0);
        }

        var output = AnimationPlayableOutput.Create(graph, "Enemy", _animator);
        output.SetSourcePlayable(mixer);
    }

    public void GameUpdate()
    {
        if (transitionProgress >= 0.0f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;

            if (transitionProgress >= 1.0f)
            {
                transitionProgress = -1.0f;
                
                SetWeight(CurrentClip, 1.0f);
                SetWeight(previousClip, 0.0f);

                GetPlayable(previousClip).Pause();
            }
            else
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1.0f - transitionProgress);
            }
        }
        
        #if UNITY_EDITOR
            clipTime = GetPlayable(CurrentClip).GetTime();
        #endif
    }

    void BeginTransition(Clip _nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = _nextClip;
        
        transitionProgress = 0.0f;

        GetPlayable(_nextClip).Play();
    }

    public void PlayIntro()
    {
        SetWeight(Clip.Intro, 1.0f);

        CurrentClip = Clip.Intro;

        graph.Play();

        transitionProgress = -1.0f;

        if (hasAppearClip)
        {
            GetPlayable(Clip.Appear).Play();
            
            SetWeight(Clip.Appear, 1.0f);
        }
    }

    public void PlayMove(float _speed)
    {
        GetPlayable(Clip.Move).SetSpeed(_speed);

        BeginTransition(Clip.Move);

        if (hasAppearClip)
        {
            SetWeight(Clip.Appear, 0.0f);
        }
    }

    public void PlayOutro()
    {
        BeginTransition(Clip.Outro);

        if (hasDisappearClip)
        {
            PlayDisappearFor(Clip.Outro);
        }
    }

    public void PlayDying()
    {
        BeginTransition(Clip.Dying);

        if (hasDisappearClip)
        {
            PlayDisappearFor(Clip.Dying);
        }
    }

    void PlayDisappearFor(Clip _otherClip)
    {
        Playable clip = GetPlayable(Clip.Disappear);
        clip.Play();
        clip.SetDelay(GetPlayable(_otherClip).GetDuration() - clip.GetDuration());

        SetWeight(Clip.Disappear, 1.0f);
    }

    Playable GetPlayable(Clip _clip) => mixer.GetInput((int) _clip);

    void SetWeight(Clip _clip, float _weight)
    {
        mixer.SetInputWeight((int) _clip, _weight);
    }

    public void Stop()
    {
        graph.Stop();
    }

    public void Destroy()
    {
        graph.Destroy();
    }
    
    #if UNITY_EDITOR
        public void RestoreAfterHotReload(Animator _animator, EnemyAnimationConfig _config, float _speed)
        {
            Configure(_animator, _config);

            GetPlayable(Clip.Move).SetSpeed(_speed);

            Playable clip = GetPlayable(CurrentClip);
            clip.SetTime(clipTime);
            clip.Play();

            SetWeight(CurrentClip, 1.0f);

            graph.Play();

            if (CurrentClip == Clip.Intro
                && hasAppearClip)
            {
                clip = GetPlayable(Clip.Appear);
                clip.SetTime(clipTime);
                clip.Play();

                SetWeight(Clip.Appear, 1.0f);
            }
            else if (CurrentClip >= Clip.Outro
                     && hasDisappearClip)
            {
                clip = GetPlayable(Clip.Disappear);
                clip.Play();

                double delay = GetPlayable(CurrentClip).GetDuration() - clip.GetDuration() - clipTime;

                if (delay >= 0.0f)
                {
                    clip.SetDelay(delay);
                }
                else
                {
                    clip.SetTime(-delay);
                }

                SetWeight(Clip.Disappear, 1.0f);
            }
        }
    #endif
}