﻿using Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using XamlBrewer.Uwp.SqLiteEntityFrameworkSample.Dal;
using XamlBrewer.Uwp.SqLiteEntityFrameworkSample.Models;

namespace XamlBrewer.Uwp.SqLiteEntityFrameworkSample.ViewModels
{
    internal class MainPageViewModel : EditableViewModel
    {
        private DelegateCommand cancelCommand;
        private DelegateCommand createCommand;
        private DelegateCommand deleteCommand;
        private bool hasSelection = false;
        private DelegateCommand newCommand;
        private ObservableCollection<PersonViewModel> persons = new ObservableCollection<PersonViewModel>();
        private DelegateCommand saveCommand;
        private DelegateCommand selectCommand;
        private PersonViewModel selectedPerson = null;
        private bool isDatabaseCreated = false;
        public MainPageViewModel()
        {
            if (this.IsInDesignMode)
            {
                return;
            }

            this.createCommand = new DelegateCommand(this.Create_Executed);
            this.selectCommand = new DelegateCommand(this.Select_Executed);
            this.newCommand = new DelegateCommand(this.New_Executed, this.New_CanExecute);
            this.deleteCommand = new DelegateCommand(this.Delete_Executed, this.Edit_CanExecute);
            this.saveCommand = new DelegateCommand(this.Save_Executed, this.Save_CanExecute);
            this.cancelCommand = new DelegateCommand(this.Cancel_Executed, this.Save_CanExecute);
        }

        public ICommand CancelCommand
        {
            get { return this.cancelCommand; }
        }

        public ICommand CreateCommand
        {
            get { return this.createCommand; }
        }

        public ICommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        public bool HasSelection
        {
            get { return this.hasSelection; }
            private set { this.SetProperty(ref this.hasSelection, value); }
        }

        public ICommand NewCommand
        {
            get { return this.newCommand; }
        }

        public ObservableCollection<PersonViewModel> Persons
        {
            get { return this.persons; }
            set { this.SetProperty(ref this.persons, value); }
        }

        public ICommand SaveCommand
        {
            get { return this.saveCommand; }
        }

        public ICommand SelectCommand
        {
            get { return this.selectCommand; }
        }
        public PersonViewModel SelectedPerson
        {
            get { return this.selectedPerson; }
            set
            {
                this.SetProperty(ref this.selectedPerson, value);
                this.HasSelection = this.selectedPerson != null;
                this.deleteCommand.RaiseCanExecuteChanged();
                this.editCommand.RaiseCanExecuteChanged();
            }
        }
        protected override bool Edit_CanExecute()
        {
            return this.selectedPerson != null && base.Edit_CanExecute();
        }

        protected override void Edit_Executed()
        {
            base.Edit_Executed();
            this.selectedPerson.IsInEditMode = true;
            this.saveCommand.RaiseCanExecuteChanged();
            this.cancelCommand.RaiseCanExecuteChanged();
        }

        private void Cancel_Executed()
        {
            if (this.selectedPerson.Id == 0)
            {
                this.persons.Remove(this.selectedPerson);
                this.SelectedPerson = null;

                // Select last person. 
                if (this.persons.Count > 0)
                {
                    this.SelectedPerson = this.Persons.Last();
                }
            }
            else
            {
                // Get old one back from db
                this.selectedPerson.Model = Repository.GetPersonById(this.selectedPerson.Id);
                this.selectedPerson.IsInEditMode = false;
            }

            this.IsInEditMode = false;
        }

        private async void Create_Executed()
        {
            await Repository.CreateDatabase();
        }

        private void Delete_Executed()
        {
            // Remove from db
            Repository.DeletePerson(this.selectedPerson.Model);

            // Remove from list
            this.Persons.Remove(this.selectedPerson);
            this.SelectedPerson = null;
        }

        private bool New_CanExecute()
        {
            return !this.IsInEditMode && this.isDatabaseCreated;
        }

        private void New_Executed()
        {
            this.Persons.Add(new PersonViewModel(new Person()));
            this.SelectedPerson = this.persons.Last();
            this.editCommand.Execute(null);
        }

        private bool Save_CanExecute()
        {
            return this.IsInEditMode;
        }

        private void Save_Executed()
        {
            // Store new one in db
            Repository.SavePerson(this.selectedPerson.Model);

            // Force a property change notification on the ViewModel:
            this.selectedPerson.Model = this.selectedPerson.Model;

            // Leave edit mode
            this.IsInEditMode = false;
            this.selectedPerson.IsInEditMode = false;
        }

        private void Select_Executed()
        {
            List<Person> models = Repository.GetAllPersons();

            this.persons.Clear();
            foreach (var m in models)
            {
                this.persons.Add(new PersonViewModel(m));
            }

            this.isDatabaseCreated = true;
            this.newCommand.RaiseCanExecuteChanged();
        }
    }
}
