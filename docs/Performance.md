The benchmark project produces useful performance benchmarks.

These are (current) raw trigger application benchmarks

|                               Method |      Mean |    Error |   StdDev |
|------------------------------------- |----------:|---------:|---------:|
|          ApplyTriggerNotmatchingType |  36.25 ns | 0.266 ns | 0.236 ns |
|    ApplyTriggerMatchingTypeAndTopics |  94.22 ns | 0.444 ns | 0.347 ns |
| ApplyTriggerMatchingTypeButNotTopics | 122.54 ns | 1.366 ns | 1.211 ns |

The vast majority of trigger matches in real life will be of the notmatchingType variety (i.e. a workflow version that is in a state that does not require a trigger of the given type.)

The second largest case will be the right-type-wrong-topics case, which is significantly slower than the not-matching-trigger.

This is comparatively expensive because it must validate all the topics and all the subjects, and is _O(n)_ over the *topics* in the trigger (the subject match is _O(1)_ as it uses a HashSet internally).

## Assumptions

- Max sustained throughput of 1,000 triggers/s (~1Mbps)
- 1/3 "matching trigger type but not topic", 2/3 "not matching trigger type"

These assumptions imply an average raw trigger processing time of *65ns*.

That implies ~*15,000* workflow subject instances per core per engine. Note that this does not include the overhead of actually performing transition.

However, this can be scaled out to arbitrarily large number of of workflow subject instances by employing subject/engine affinity and scaling.