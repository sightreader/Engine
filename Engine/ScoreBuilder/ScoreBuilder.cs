using MusicXmlSchema;
using SightReader.Engine.Errors;
using SightReader.Engine.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

namespace SightReader.Engine.ScoreBuilder
{
    public static class StringExtensions
    {
        public static byte ToByte(this string s)
        {
            byte.TryParse(s.Replace("[^0-9]", ""), out byte result);
            return result;
        }

        public static int ToInt(this string s)
        {
            int.TryParse(s.Replace("[^0-9]", ""), out int result);
            return result;
        }
    }

    public static class MusicXmlPitchExtensions
    {
        public static byte ToByte(this pitch p)
        {
            byte OCTAVE_SEMITONES = 12;
            byte C0_VALUE = 12; /* MIDI defines C0 as 12 */
            byte octave = p.octave.ToByte();

            byte valueForOctave = (byte)(C0_VALUE + octave * OCTAVE_SEMITONES);

            byte stepSemitone = p.step switch
            {
                step.C => 0,
                step.D => 2,
                step.E => 4,
                step.F => 5,
                step.G => 7,
                step.A => 9,
                step.B => 11,
                _ => throw new InvalidMusicXmlDocumentException(null, $"Received a pitch with a step of {p.step.ToString()}")
            };

            /* alter is a decimal, but microtonal alters aren't supported in a piano */
            decimal alterSemitone = (p.alterSpecified || p.alter != 0) ? Math.Floor(p.alter) : 0;

            return (byte)(valueForOctave + stepSemitone + alterSemitone);
        }
    }

    public static class ObjectModelCollectionsExtensions
    {
        public static T[] ToArray<T>(this System.Collections.ObjectModel.Collection<T> c)
        {
            var array = new T[] { };
            c.CopyTo(array, 0);
            return array;
        }
    }

    public class ScoreBuilder
    {
        private Stream stream;

        public ScoreBuilder(Stream stream)
        {
            this.stream = stream;
        }

        public Score Build()
        {
            var rawScore = LoadFromMusicXml();
            var builtScore = BuildScore(rawScore);

            return builtScore;
        }

        private scorepartwise LoadFromMusicXml()
        {
            var serializer = new XmlSerializer(typeof(scorepartwise));

            try
            {
                return (scorepartwise)serializer.Deserialize(stream);
            }
            catch (Exception ex) when (ex.InnerException is XmlException)
            {
                throw new InvalidMusicXmlDocumentException(ex.InnerException as XmlException);
            }
        }

        private Score BuildScore(scorepartwise rawScore)
        {
            var builtScore = new Score()
            {
                Info = BuildScoreInfo(rawScore),
                Parts = rawScore.part.Select(x => new Part()
                {
                    Id = x.id,
                    Staves = BuildPartStaves(rawScore, x.measure.ToArray())
                }).ToArray()
            };
            return builtScore;
        }

        private ScoreInfo BuildScoreInfo(scorepartwise score)
        {
            var info = new ScoreInfo();

            if (score.work != null)
            {
                if (score.work.worktitle != null)
                {
                    info.WorkTitle = score.work.worktitle;
                }
                if (score.work.worknumber != null)
                {
                    info.WorkNumber = score.work.worknumber;
                }
            }

            if (score.movementnumber != null)
            {
                info.MovementNumber = score.movementnumber;
            }
            if (score.movementtitle != null)
            {
                info.MovementTitle = score.movementtitle;
            }

            if (score.identification != null)
            {
                if (score.identification.creator?.Length > 0)
                {
                    info.Creators = score.identification.creator.Select(x => new ScoreCreator() { Type = x.type ?? "", Name = x.Value ?? "" }).ToArray();
                }
                if (score.identification.rights?.Length > 0)
                {
                    info.Rights = score.identification.rights.Select(x => new ScoreRights() { Type = x.type ?? "", Content = x.Value ?? "" }).ToArray();
                }
                if (score.identification.miscellaneous?.Length > 0)
                {
                    info.Misc = score.identification.miscellaneous.Select(x => new ScoreMisc() { Key = x.name ?? "", Value = x.Value ?? "" }).ToArray();
                }
                if (score.identification.source != null)
                {
                    info.Source = score.identification.source;
                }
                if (score.identification.encoding?.Items.Length > 0)
                {
                    var encodingSoftware = score.identification.encoding.Items.Where((x, i) => score.identification.encoding.ItemsElementName[i] == ItemsChoiceType.software).Select(x => x.ToString()).ToArray();
                    var encodingDates = score.identification.encoding.Items.Where((x, i) => score.identification.encoding.ItemsElementName[i] == ItemsChoiceType.encodingdate).OfType<DateTime>().ToArray();

                    if (encodingSoftware.Length > 0)
                    {
                        info.EncodingSoftware = encodingSoftware;
                    }
                    if (encodingDates.Length > 0)
                    {
                        info.EncodingDates = encodingDates;
                    }
                }
            }

            if (score.credit?.Length > 0)
            {
                info.Credits = score.credit.SelectMany(credit => credit.Items.OfType<formattedtextid>().Select(creditWords => creditWords.Value ?? "")).ToArray();
            }

            return info;
        }

        private Staff[] BuildPartStaves(scorepartwise rawScore, scorepartwisePartMeasure[] measures)
        {
            var elementStaves = new List<List<List<IElement>>>(2)
            {
                new List<List<IElement>>(),
                new List<List<IElement>>()
            };
            var beatDurationDirectives = new List<BeatDurationDirective>();
            var repeatDirectives = new List<RepeatDirective>();

            foreach (var measure in measures)
            {
                BuildPartStaffMeasure(measure, elementStaves, beatDurationDirectives, repeatDirectives);
            }

            return elementStaves.Select((x, i) => new Staff()
            {
                Number = i + 1,
                Directives = new List<IDirective>().Concat(beatDurationDirectives)
                .Concat(repeatDirectives).ToArray(),
                Elements = x.Select(y => y.ToArray()).ToArray()
            }).Where(x => x.Elements.Length > 0).ToArray();
        }

        private void BuildPartStaffMeasure(scorepartwisePartMeasure measure,  List<List<List<IElement>>> elementStaves, List<BeatDurationDirective> beatDurationDirectives, List<RepeatDirective> repeatDirectives)
        {
            var measureNumber = measure.number.ToInt();
            var staffBuilders = new StaffBuilder[] {
                new StaffBuilder(1),
                new StaffBuilder(2)
            };

            foreach (var item in measure.Items)
            {
                switch (item)
                {
                    case attributes attributes:
                        BuildPartStaffMeasureAttributes(attributes, beatDurationDirectives, measureNumber);
                        break;
                    case barline barline:
                        BuildPartStaffMeasureBarline(barline, repeatDirectives, measureNumber);
                        break;
                    case note rawNote:
                        Element el = BuildPartStaffMeasureElement(rawNote, measureNumber);
                        /* If there are more than 2 staves, dynamically extend the array */
                        if (el.Staff > staffBuilders.Length)
                        {
                            var staffBuildersExtended = new List<StaffBuilder>(staffBuilders);
                            staffBuildersExtended.Add(new StaffBuilder(staffBuilders.Length));
                            staffBuilders = staffBuildersExtended.ToArray();
                        }

                        for (int i = 0; i < staffBuilders.Length; i++)
                        {
                            staffBuilders[i].ProcessNote(el);
                        }
                        break;
                    case backup backup:
                        for (int i = 0; i < staffBuilders.Length; i++)
                        {
                            staffBuilders[i].RewindClock(backup.duration);
                        }
                        break;
                    case forward forward:
                        if (forward.voice?.Length > 0)
                        {
                            throw new NotSupportedException($"Measure {measureNumber} has a <forward> element with voice {forward.voice}. Forwarding specific voices is not currently supported.");
                        }

                        if (forward.staff?.Length > 0)
                        {
                            var staffForClock = forward.staff.ToByte() - 1;
                            staffBuilders[staffForClock].AdvanceClock(forward.duration);
                        }
                        else
                        {
                            for (int i = 0; i < staffBuilders.Length; i++)
                            {
                                staffBuilders[i].RewindClock(forward.duration);
                            }
                        }
                        break;
                }
            }

            for (int i = 0; i < staffBuilders.Length; i++)
            {
                var staffBuilder = staffBuilders[i];
                var elementStaff = elementStaves[i];
                elementStaff.AddRange(staffBuilder.GetElements());
            }
        }

        private void BuildPartStaffMeasureAttributes(attributes attributes, List<BeatDurationDirective> beatDurationDirectives, int measureNumber)
        {
            if (attributes.divisionsSpecified)
            {
                var start = measureNumber;
                var end = int.MaxValue;

                if (beatDurationDirectives.Count > 0)
                {
                    var lastBeatDurationDirective = beatDurationDirectives.Last();
                    lastBeatDurationDirective.ElementRange = new Range(lastBeatDurationDirective.ElementRange.Start, start);
                }

                beatDurationDirectives.Add(new BeatDurationDirective()
                {
                    Divisions = attributes.divisions,
                    ElementRange = new Range(start, end)
                });
            }
        }

        private void BuildPartStaffMeasureBarline(barline barline, List<RepeatDirective> repeatDirectives, int measureNumber)
        {
            switch (barline.repeat?.direction)
            {
                case backwardforward.backward:
                    repeatDirectives.Add(new RepeatDirective()
                    {
                        ElementRange = new Range(measureNumber, int.MaxValue)
                    });
                    break;
                case backwardforward.forward:
                    if (repeatDirectives.Count > 0)
                    {
                        var lastRepeatDirective = repeatDirectives.Last();
                        lastRepeatDirective.ElementRange = new Range(lastRepeatDirective.ElementRange.Start, measureNumber);
                    }
                    break;
            }
        }

        private Element BuildPartStaffMeasureElement(note rawNote, int measureNumber)
        {
            var el = new Element();
            var notations = new List<INotation>();

            el.Measure = (ushort)measureNumber;
            
            if (rawNote.staff != null)
            {
                el.Staff = rawNote.staff.ToByte();
            }
            if (rawNote.voice != null)
            {
                el.Voice = rawNote.voice.ToByte();
            }

            if (rawNote.notations != null)
            {
                BuildPartStaffMeasureElementNotations(rawNote.notations, notations);
                el.Notations = notations.ToArray();
            }

            for (int i = 0; i < rawNote.Items.Length; i++)
            {
                var rawItemType = rawNote.ItemsElementName[i];

                switch (rawItemType)
                {
                    case ItemsChoiceType1.duration:
                        var duration = (decimal)rawNote.Items[i];
                        el.Duration = duration;

                        if (el.IsGraceNote)
                        {
                            el.Duration = Math.Max(1, el.Duration);
                        }
                        break;
                    case ItemsChoiceType1.grace:
                        el.IsGraceNote = true;
                        /* Grace notes have a default duration of 0. This would cause them
                         * to be interpreted as a chord (in the same group as the next note).
                         * So we assign a minimum duration of 1. */
                        el.Duration = Math.Max(1, el.Duration);
                        break;
                    case ItemsChoiceType1.rest:
                        el.IsRest = true;
                        break;
                    case ItemsChoiceType1.chord:
                        el.IsChordChild = true;
                        break;
                    case ItemsChoiceType1.pitch:
                        var pitch = rawNote.Items[i] as pitch;
                        el.Pitch = pitch!.ToByte();

                        var alter = pitch!.alterSpecified ? pitch.alter switch {
                            -2 => "bb",
                            -1 => "b",
                            0 => "",
                            1 => "#",
                            2 => "##",
                            _ => ""
                        } : "";

                        el.NotatedPitch = $"{pitch.step.ToString().ToUpper()}{alter}{pitch.octave}";
                        break;
                }
            }

            return el;
        }

        private void BuildPartStaffMeasureElementNotations(notations[] rawNotations, List<INotation> notations)
        {
            foreach (var rawNotation in rawNotations)
            {
                foreach (var item in rawNotation.Items)
                {
                    switch (item)
                    {
                        case arpeggiate arpeggiate:
                            notations.Add(new Arpeggiate()
                            {
                                IsDownwards = arpeggiate.direction == updown.down,
                                Number = arpeggiate.number != null ? arpeggiate.number.ToByte() : (byte)1
                            });
                            break;
                        case articulations articulations:
                            BuildPartStaffMeasureElementArticulations(articulations, notations);
                            break;
                        case glissando glissando:
                            notations.Add(new Glissando()
                            {
                                IsStarting = glissando.type == startstop.start,
                                Number = glissando.number != null ? glissando.number.ToByte() : (byte)1
                            });
                            break;
                        case nonarpeggiate nonArpeggiate:
                            notations.Add(new NonArpeggiate()
                            {
                                IsTop = nonArpeggiate.type == topbottom.top,
                                Number = nonArpeggiate.number != null ? nonArpeggiate.number.ToByte() : (byte)1
                            });
                            break;
                        case ornaments ornaments:
                            BuildPartStaffMeasureElementOrnaments(ornaments, notations);
                            break;
                        case slide slide:
                            notations.Add(new Slide()
                            {
                                IsStarting = slide.type == startstop.start,
                                Number = slide.number != null ? slide.number.ToByte() : (byte)1
                            });
                            break;
                        case slur slur:
                            notations.Add(new Slur()
                            {
                                Type = slur.type switch
                                {
                                    startstopcontinue.@continue => StartStopContinue.Continue,
                                    startstopcontinue.start => StartStopContinue.Start,
                                    startstopcontinue.stop => StartStopContinue.Stop,
                                    _ => StartStopContinue.Stop
                                },
                                Number = slur.number != null ? slur.number.ToByte() : (byte)1
                            });
                            break;
                        case tied tied:
                            notations.Add(new Tie()
                            {
                                Type = tied.type switch
                                {
                                    tiedtype.@continue => StartStopContinue.Continue,
                                    tiedtype.start => StartStopContinue.Start,
                                    tiedtype.stop => StartStopContinue.Stop,
                                    _ => StartStopContinue.Stop
                                },
                                Number = tied.number != null ? tied.number.ToByte() : (byte)1,
                            });
                            break;
                    }
                }
            }
        }

        private void BuildPartStaffMeasureElementArticulations(articulations rawArticulations, List<INotation> notations)
        {
            for (int i = 0; i < rawArticulations.Items.Length; i++)
            {
                var rawArticulationType = rawArticulations.ItemsElementName[i];

                switch (rawArticulationType)
                {
                    case ItemsChoiceType4.accent:
                        notations.Add(new Articulation() { IsAccent = true });
                        break;
                    case ItemsChoiceType4.caesura:
                        notations.Add(new Articulation() { IsCaesura = true });
                        break;
                    case ItemsChoiceType4.detachedlegato:
                        notations.Add(new Articulation() { IsDetachedLegato = true });
                        break;
                    case ItemsChoiceType4.doit:
                        notations.Add(new Articulation() { IsDoit = true });
                        break;
                    case ItemsChoiceType4.falloff:
                        notations.Add(new Articulation() { IsFalloff = true });
                        break;
                    case ItemsChoiceType4.plop:
                        notations.Add(new Articulation() { IsPlop = true });
                        break;
                    case ItemsChoiceType4.scoop:
                        notations.Add(new Articulation() { IsScoop = true });
                        break;
                    case ItemsChoiceType4.spiccato:
                        notations.Add(new Articulation() { IsSpiccato = true });
                        break;
                    case ItemsChoiceType4.staccatissimo:
                        notations.Add(new Articulation() { IsStaccatissimo = true });
                        break;
                    case ItemsChoiceType4.staccato:
                        notations.Add(new Articulation() { IsStaccato = true });
                        break;
                    case ItemsChoiceType4.stress:
                        notations.Add(new Articulation() { IsStressed = true });
                        break;
                    case ItemsChoiceType4.strongaccent:
                        notations.Add(new Articulation() { IsStrongAccent = true });
                        break;
                    case ItemsChoiceType4.tenuto:
                        notations.Add(new Articulation() { IsTenuto = true });
                        break;
                    case ItemsChoiceType4.unstress:
                        notations.Add(new Articulation() { IsUnstresed = true });
                        break;
                }
            }
        }

        private void BuildPartStaffMeasureElementOrnaments(ornaments ornaments, List<INotation> notations)
        {
            for (int i = 0; i < ornaments.Items.Length; i++)
            {
                var ornamentType = ornaments.ItemsElementName[i];

                switch (ornamentType)
                {
                    case ItemsChoiceType2.delayedinvertedturn:
                        var delayedInvertedTurn = ornaments.Items[i] as horizontalturn;
                        notations.Add(new Turn()
                        {
                            IsDelayed = true,
                            IsInverted = true,
                            StartNote = delayedInvertedTurn!.startnoteSpecified ? delayedInvertedTurn.startnote switch
                            {
                                startnote.below => StartNote.Below,
                                startnote.main => StartNote.Main,
                                startnote.upper => StartNote.Upper,
                                _ => StartNote.Below
                            } : StartNote.Below
                        });
                        break;
                    case ItemsChoiceType2.delayedturn:
                        var delayedTurn = ornaments.Items[i] as horizontalturn;
                        notations.Add(new Turn()
                        {
                            IsDelayed = true,
                            IsInverted = false,
                            StartNote = delayedTurn!.startnoteSpecified ? delayedTurn.startnote switch
                            {
                                startnote.below => StartNote.Below,
                                startnote.main => StartNote.Main,
                                startnote.upper => StartNote.Upper,
                                _ => StartNote.Upper
                            } : StartNote.Upper
                        });
                        break;
                    case ItemsChoiceType2.invertedturn:
                        var invertedTurn = ornaments.Items[i] as horizontalturn;
                        notations.Add(new Turn()
                        {
                            IsDelayed = false,
                            IsInverted = true,
                            StartNote = invertedTurn!.startnoteSpecified ? invertedTurn.startnote switch
                            {
                                startnote.below => StartNote.Below,
                                startnote.main => StartNote.Main,
                                startnote.upper => StartNote.Upper,
                                _ => StartNote.Below
                            } : StartNote.Below
                        });
                        break;
                    case ItemsChoiceType2.turn:
                        var turn = ornaments.Items[i] as horizontalturn;
                        notations.Add(new Turn()
                        {
                            IsDelayed = false,
                            IsInverted = false,
                            StartNote = turn!.startnoteSpecified ? turn.startnote switch
                            {
                                startnote.below => StartNote.Below,
                                startnote.main => StartNote.Main,
                                startnote.upper => StartNote.Upper,
                                _ => StartNote.Upper
                            } : StartNote.Upper
                        });
                        break;
                    case ItemsChoiceType2.mordent:
                        notations.Add(new Mordent()
                        {
                            IsInverted = false
                        });
                        break;
                    case ItemsChoiceType2.invertedmordent:
                        notations.Add(new Mordent()
                        {
                            IsInverted = true
                        });
                        break;
                    case ItemsChoiceType2.schleifer:
                        notations.Add(new Schleifer());
                        break;
                    case ItemsChoiceType2.shake:
                        notations.Add(new Shake());
                        break;
                    case ItemsChoiceType2.trillmark:
                        notations.Add(new Trill());
                        break;
                    case ItemsChoiceType2.tremolo:
                        var tremolo = ornaments.Items[i] as tremolo;
                        notations.Add(new Tremolo()
                        {
                            Type = tremolo!.type switch
                            {
                                tremolotype.start => TremoloType.Start,
                                tremolotype.stop => TremoloType.Stop,
                                tremolotype.single => TremoloType.Single,
                                _ => TremoloType.Single,
                            }
                        });
                        break;
                    case ItemsChoiceType2.wavyline:
                        var wavyline = ornaments.Items[i] as wavyline;
                        notations.Add(new WavyLine()
                        {
                            Number = wavyline!.number != null ? wavyline.number.ToByte() : (byte)1,
                            StartNote = wavyline.startnoteSpecified ?
                            wavyline.startnote switch
                            {
                                startnote.below => StartNote.Below,
                                startnote.main => StartNote.Main,
                                startnote.upper => StartNote.Upper,
                                _ => StartNote.Main,
                            } : StartNote.Main,
                            TrillStep = wavyline.trillstepSpecified ?
                            wavyline.trillstep switch
                            {
                                trillstep.half => TrillStep.Half,
                                trillstep.unison => TrillStep.Unison,
                                trillstep.whole => TrillStep.Whole,
                                _ => TrillStep.Unison
                            } : TrillStep.Unison,
                            TwoNoteTurn = wavyline.twonoteturnSpecified ?
                            wavyline.twonoteturn switch
                            {
                                twonoteturn.half => TwoNoteTurn.Half,
                                twonoteturn.none => TwoNoteTurn.None,
                                twonoteturn.whole => TwoNoteTurn.Whole,
                                _ => TwoNoteTurn.None
                            } : TwoNoteTurn.None,
                            Type = wavyline.type switch
                            {
                                startstopcontinue.@continue => StartStopContinue.Continue,
                                startstopcontinue.start => StartStopContinue.Start,
                                startstopcontinue.stop => StartStopContinue.Stop,
                                _ => StartStopContinue.Stop
                            }
                        });
                        break;
                }
            }
        }
    }
}