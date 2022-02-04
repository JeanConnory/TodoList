using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoList.Banco;
using TodoList.Modelos;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TodoList.Telas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Cadastrar : ContentPage
    {
        public string Prioridade;

        public Cadastrar()
        {
            InitializeComponent();
            lblData.Text = DateTime.Now.ToString("dd/MM/yyyy");
            rbNormal.IsChecked = true;
        }

        private void FecharModal_Tapped(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private async void SalvarTarefa(object sender, EventArgs e)
        {
            Tarefa tarefa = new Tarefa();
            tarefa.Nome = Nome.Text;
            tarefa.Nota = Nota.Text;
            tarefa.Data = Data.Date;
            tarefa.HorarioInicial = HorarioInicial.Time;
            tarefa.HorarioFinal = HorarioFinal.Time;
            tarefa.Finalizada = false;
            tarefa.Prioridade = this.Prioridade;

            if (await ValidacaoAsync(tarefa))
            {
                if (await new TarefaDB().CadastrarAsync(tarefa))
                {
                    MessagingCenter.Send<Listar, Tarefa>(new Listar(), "OnTarefaCadastrada", tarefa);
                    await Navigation.PopModalAsync();
                }
            }
        }

        private async Task<bool> ValidacaoAsync(Tarefa tarefa)
        {
            bool validacao = true;

            if(tarefa.HorarioInicial > tarefa.HorarioFinal)
            {
                await DisplayAlert("Erro", "O horário inicial não pode ser maior que horário de término!", "Ok");
                validacao = false;
            }

            if (tarefa.Nome == null)
            {
                await DisplayAlert("Erro", "O nome da tarefa precisa ser preenchido!", "OK");
                validacao = false;
            }

            if (tarefa.Nome != null && tarefa.Nome.Length < 5)
            {
                await DisplayAlert("Erro", "O nome da tarefa precisa ter pelo menos 5 caracteres!", "OK");
                validacao = false;
            }

            return validacao;
        }

        private void Data_Unfocused(object sender, FocusEventArgs e)
        {
            lblData.Text = ((DatePicker)sender).Date.ToString("dd/MM/yyyy");
        }

        private void AcionarDatePicker(object sender, EventArgs e)
        {
            Data.Focus();
        }

        private void AcionarTimePicker(object sender, EventArgs e)
        {
            HorarioInicial.Focus();
        }

        private void HorarioInicial_Unfocused(object sender, FocusEventArgs e)
        {
            lblHorarioInicial.Text = ((TimePicker)sender).Time.ToString(@"hh\:mm");
            HorarioFinal.Focus();
        }

        private void HorarioFinal_Unfocused(object sender, FocusEventArgs e)
        {
            lblHorarioFinal.Text = ((TimePicker)sender).Time.ToString(@"hh\:mm");
        }

        private void rbBaixa_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Prioridade = ((RadioButton)sender).Text;
        }
    }
}