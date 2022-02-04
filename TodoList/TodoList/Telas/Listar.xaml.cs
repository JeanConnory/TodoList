using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TodoList.Banco;
using TodoList.Modelos;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TodoList.Telas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Listar : ContentPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Tarefa> _lista;
        public ObservableCollection<Tarefa> Lista
        {
            get
            {
                return _lista;
            }
            set
            {
                _lista = value;
                NotifyPropertyChanged("Lista");
            }
        }

        public Listar()
        {
            InitializeComponent();

            AtualizarDataCalendario(DateTime.Now);

            MessagingCenter.Subscribe<Listar, Tarefa>(this, "OnTarefaCadastrada", (sender, tarefa) =>
            {
                if (Lista != null)
                {
                    if (DPCalendario.Date == tarefa.Data)
                    {
                        Lista.Add(tarefa);
                    }
                }
            });
        }

        private void btnCadastrar_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new Cadastrar());
        }

        private void BtnVisualizar(object sender, EventArgs e)
        {
            var evento = (TappedEventArgs)e;
            var tarefa = (Tarefa)evento.Parameter;

            Navigation.PushAsync(new Visualizar(tarefa));
        }

        private void AtualizarDataCalendario(DateTime data)
        {
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Lista = new ObservableCollection<Tarefa>(
                        await new TarefaDB().PesquisarAsync(data)
                    );
                    CVListaDeTarefas.ItemsSource = Lista;
                    QuantidadeTarefas.Text = Lista.Count.ToString();
                });
            });

            var idioma = CultureInfo.GetCultureInfo("pt-Br");

            Dia.Text = data.Day.ToString();
            Mes.Text = PrimeiraLetraMaiuscula(data.ToString("MMMM", idioma).Substring(0, 3));
            DiaDaSemana.Text = PrimeiraLetraMaiuscula(idioma.DateTimeFormat.GetDayName(data.DayOfWeek));
        }

        private string PrimeiraLetraMaiuscula(string palavra)
        {
            return char.ToUpper(palavra[0]) + palavra.Substring(1);
        }

        private void AbrirCalendario(object sender, EventArgs e)
        {
            DPCalendario.Focus();
        }

        private void DateSelectedAction(object sender, DateChangedEventArgs e)
        {
            AtualizarDataCalendario(e.NewDate);
        }

        private async void BtnExcluir(object sender, EventArgs e)
        {
            bool pergunta = await DisplayAlert("Excluir", "Tem certeza que deseja excluir este registro?", "Sim", "Não");

            if (pergunta)
            {
                var swipeItem = (SwipeItem)sender;
                Tarefa tarefa = (Tarefa)swipeItem.CommandParameter;

                var excluido = await new TarefaDB().ExcluirAsync(tarefa.Id);

                if (excluido)
                {
                    Lista.Remove(tarefa);
                }
            }
        }

        private async void CheckedAction(object sender, CheckedChangedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var grid = (Grid)checkbox.Parent;
            var stacklayout = (StackLayout)grid.Children[1];
            var tapGesture = (TapGestureRecognizer)stacklayout.GestureRecognizers[0];
            var tarefa = (Tarefa)tapGesture.CommandParameter;
            var label = (Label)stacklayout.Children[0];
            
            if (tarefa != null)
            {
                await new TarefaDB().AtualizarAsync(tarefa);
                Tachado(label, tarefa.Finalizada);
            }
        }

        private void Tachado(Label label, bool finalizada)
        {
            if (finalizada)
            {
                label.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                label.TextDecorations = TextDecorations.None;
            }
        }
    }
}