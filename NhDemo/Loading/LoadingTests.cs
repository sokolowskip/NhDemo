using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentNHibernate.Mapping;
using NhDemo.Configuration;
using NHibernate.Mapping.ByCode;
using NHibernate.Transform;
using Xunit;
using Xunit.Abstractions;

namespace NhDemo.Loading
{
    public class LoadingTests
    {
        private const int ItemsCount = 8;
        private readonly ITestOutputHelper _output;
        private readonly long _windowId;

        public LoadingTests(ITestOutputHelper output)
        {
            _output = output;

            using (var session = SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                var window = new Window();
                window.Name = "MainWindow";
                window.Labels = new HashSet<Label>();
                for (int i = 0; i < ItemsCount; i++)
                {
                    window.Labels.Add(new Label {Content = "label " + i});
                }
                window.Dialogs = new HashSet<Dialog>();
                for (int i = 0; i < ItemsCount; i++)
                {
                    var dialog = new Dialog
                    {
                        Content = "content" + i,
                        Type = i,
                        Buttons = new HashSet<Button>()
                    };
                    for (int j = 0; j < ItemsCount; j++)
                    {
                        dialog.Buttons.Add(new Button
                        {
                            Action = "a " + i*j,
                            Text = "text " + i + j,
                        });
                    }
                    window.Dialogs.Add(dialog);
                }
                window.Buttons = new HashSet<Button>();
                for (int i = 0; i < ItemsCount; i++)
                {
                    window.Buttons.Add(new Button
                    {
                        Action = "acc " + i,
                        Text = "teeext " + i*2
                    });
                }
                window.Inputs = new HashSet<Input>();
                for (int i = 0; i < ItemsCount; i++)
                {
                    var input = new Input
                    {
                        Type = i,
                        Value = (i*i*i - i*i).ToString(),
                        Borders = new HashSet<Border>(),
                        Elements = new HashSet<InputElement>()
                    };
                    for (int j = 0; j < ItemsCount; j++)
                    {
                        input.Borders.Add(new Border
                        {
                            Thickness = 0.1 * i * j
                        });
                    }
                    for (int j = 0; j < ItemsCount; j++)
                    {
                        input.Elements.Add(new InputElement {Implementator = new Implementator {ImplementationName = "Impl" + (i*i-2*i+i).ToString()} });
                    }
                    window.Inputs.Add(input);
                }
                window.LeftBorder = new Border {Thickness = 11.0};
                window.TopBorder = new Border {Thickness = 18.1};
                window.RightBorder = new Border {Thickness = 12.6};
                window.BottomBorder = new Border {Thickness = 7};

                session.Save(window);
                session.Flush();
                tran.Commit();

                _windowId = window.Id;
            }
        }

        [Fact]
        public void GetTest()
        {
            using (var session = SessionFactory.OpenSession())
            {
                Stopwatch watch = Stopwatch.StartNew();
                var window = session.Get<Window>(_windowId);
                
                var creationCost = window.CreationCost;
                watch.Stop();
                _output.WriteLine($"Creation cost: {creationCost}");
                _output.WriteLine($"Time: {watch.Elapsed}");
            }
        }

        // Dla 10 trwa to bardzo długo
        [Fact]
        public void EagerFetchTest()
        {
            using (var session = SessionFactory.OpenSession())
            {
                Stopwatch watch = Stopwatch.StartNew();
                var window = session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.LeftBorder).Eager
                    .Fetch(x => x.RightBorder).Eager
                    .Fetch(x => x.TopBorder).Eager
                    .Fetch(x => x.BottomBorder).Eager
                    .Fetch(x => x.Labels).Eager
                    .Fetch(x => x.Dialogs).Eager
                    .Fetch(x => x.Dialogs.First().Buttons).Eager
                    .Fetch(x => x.Buttons).Eager
                    .Fetch(x => x.Inputs).Eager
                    .Fetch(x => x.Inputs.First().Borders).Eager
                    .Fetch(x => x.Inputs.First().Elements).Eager
                    .Fetch(x => x.Inputs.First().Elements.First().Implementator).Eager
                    .TransformUsing(Transformers.DistinctRootEntity)
                    .SingleOrDefault();

                
                var creationCost = window.CreationCost;
                watch.Stop();
                _output.WriteLine($"Creation cost: {creationCost}");
                _output.WriteLine($"Time: {watch.Elapsed}");
            }
        }

        [Fact]
        public void EagerFetchFutureTest()
        {
            using (var session = SessionFactory.OpenSession())
            {
                Stopwatch watch = Stopwatch.StartNew();
                var windows = session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.LeftBorder).Eager
                    .Fetch(x => x.RightBorder).Eager
                    .Fetch(x => x.TopBorder).Eager
                    .Fetch(x => x.BottomBorder).Eager
                    .Future();
                session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.Labels).Eager
                    .Future();
                session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.Dialogs).Eager
                    .Fetch(x => x.Dialogs.First().Buttons).Eager
                    .Future();
                session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.Buttons).Eager
                    .Future();
                session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.Inputs).Eager
                    .Fetch(x => x.Inputs.First().Borders).Eager
                    .Future();
                session.QueryOver<Window>()
                    .Where(x => x.Id == _windowId)
                    .Fetch(x => x.Inputs).Eager
                    .Fetch(x => x.Inputs.First().Elements).Eager
                    .Fetch(x => x.Inputs.First().Elements.First().Implementator).Eager
                    .Future();

                var window = windows.Single();
                var creationCost = window.CreationCost;
                watch.Stop();
                _output.WriteLine($"Creation cost: {creationCost}");
                _output.WriteLine($"Time: {watch.Elapsed}");
            }
        }
    }

    public interface IMeasurable
    {
        double CreationCost { get; }
    }

    public class Window : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual ISet<Label> Labels { get; set; }

        public virtual ISet<Dialog> Dialogs { get; set; }

        public virtual ISet<Button> Buttons { get; set; }

        public virtual ISet<Input> Inputs { get; set; }

        public virtual Border TopBorder { get; set; }
        public virtual Border BottomBorder { get; set; }
        public virtual Border LeftBorder { get; set; }
        public virtual Border RightBorder { get; set; }

        public virtual double CreationCost =>
            Labels.Sum(x => x.CreationCost) +
            Dialogs.Sum(x => x.CreationCost) +
            Buttons.Sum(x => x.CreationCost) +
            Inputs.Sum(x => x.CreationCost) +
            (TopBorder?.CreationCost ?? 0.0) +
            (LeftBorder?.CreationCost ?? 0.0) +
            (RightBorder?.CreationCost ?? 0.0) +
            (BottomBorder?.CreationCost ?? 0.0) +
            (Name?.Length ?? 0.0);
    }

    public class WindowMap : ClassMap<Window>
    {
        public WindowMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.Name);
            References(x => x.TopBorder).Cascade.All();
            References(x => x.RightBorder).Cascade.All();
            References(x => x.BottomBorder).Cascade.All();
            References(x => x.LeftBorder).Cascade.All();
            HasMany(x => x.Labels).Cascade.All();
            HasMany(x => x.Dialogs).Cascade.All();
            HasMany(x => x.Buttons).Cascade.All();
            HasMany(x => x.Inputs).Cascade.All();
        }
    }

    public class Label : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual string  Content { get; set; }

        public virtual double CreationCost => Content?.Length ?? 0.0;
    }

    public class LabelMap : ClassMap<Label>
    {
        public LabelMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.Content);
        }
    }

    public class Dialog : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual int Type { get; set; }

        public virtual string Content { get; set; }

        public virtual ISet<Button> Buttons { get; set; }

        public virtual double CreationCost => Buttons.Sum(x => x.CreationCost) + (Content?.Length ?? 0.0)*1.5;
    }

    public class DialogMap : ClassMap<Dialog>
    {
        public DialogMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.Type);
            Map(x => x.Content);
            HasMany(x => x.Buttons).Cascade.All();
        }
    }

    public class Button : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual string Text { get; set; }

        public virtual string Action { get; set; }

        public virtual double CreationCost => (Text?.Length ?? 0.0)/2.0 + (Action?.Length ?? 0.0);
    }


    public class ButtonMap : ClassMap<Button>
    {
        public ButtonMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.Text);
            Map(x => x.Action);
        }
    }

    public class Input : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual int Type { get; set; }

        public virtual string Value { get; set; }

        public virtual ISet<Border> Borders { get; set; }

        public virtual ISet<InputElement> Elements { get; set; }

        public virtual double CreationCost => (Value?.Length ?? 1.0) + Borders.Sum(x => x.CreationCost) + Elements.Sum(x => x.CreationCost);
    }

    public class InputMap : ClassMap<Input>
    {
        public InputMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.Type);
            Map(x => x.Value);
            HasMany(x => x.Borders).Cascade.All();
            HasMany(x => x.Elements).Cascade.All();
        }
    }

    public class Border : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual double Thickness { get; set; }

        public virtual double CreationCost => Thickness*0.8;
    }

    public class BorderMap : ClassMap<Border>
    {
        public BorderMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.Thickness);
        }
    }

    public class InputElement : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual Implementator Implementator { get; set; }

        public virtual double CreationCost => 1.0 + Implementator?.CreationCost ?? 0.5;
    }

    public class InputElementMap : ClassMap<InputElement>
    {
        public InputElementMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            References(x => x.Implementator).Cascade.All();
        }
    }

    public class Implementator : IMeasurable
    {
        public virtual long Id { get; set; }

        public virtual string  ImplementationName{ get; set; }

        public virtual double CreationCost => ImplementationName?.Length ?? 0.0;
    }

    public class ImplentatorMap : ClassMap<Implementator>
    {
        public ImplentatorMap()
        {
            Id(x => x.Id).GeneratedBy.HiLo("9000");
            Map(x => x.ImplementationName);
        }
    }
}