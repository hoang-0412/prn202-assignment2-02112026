using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ASM2_HE194435.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM2_HE194435;

public partial class ProjectWindow : Window
{
    private bool _isInternalChange = false;

    public ProjectWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadProjects();
        LoadTypes();
        LoadDepartments();
    }

    private void LoadProjects()
    {
        try
        {
            using var context = new Assigment2DbContext();
            dgProjects.ItemsSource = context.Projects
                .Include(p => p.DepartmentNavigation)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading projects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTypes()
    {
        try
        {
            _isInternalChange = true;
            using var context = new Assigment2DbContext();
            var types = context.Projects
                .Where(p => !string.IsNullOrEmpty(p.Type))
                .Select(p => p.Type!)
                .Distinct()
                .ToList();
            cboType.ItemsSource = types;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading project types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isInternalChange = false;
        }
    }

    private void LoadDepartments()
    {
        try
        {
            using var context = new Assigment2DbContext();
            cboDepartment.ItemsSource = context.Departments.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading departments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidateInput()
    {
        string projectName = txtProjectName.Text.Trim();
        if (string.IsNullOrWhiteSpace(projectName))
        {
            MessageBox.Show("Project Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!char.IsUpper(projectName[0]))
        {
            MessageBox.Show("Project Name must start with an uppercase letter.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (dpStartDate.SelectedDate == null)
        {
            MessageBox.Show("Start Date is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (dpStartDate.SelectedDate.Value.Year != 2025)
        {
            MessageBox.Show("Start Date must be in the year 2025.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (cboType.SelectedItem == null)
        {
            MessageBox.Show("Type must be selected.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private bool ConfirmOptionalFields()
    {
        if (cboDepartment.SelectedValue == null)
        {
            MessageBoxResult confirmDept = MessageBox.Show(
                "Department is empty. Do you want to continue?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmDept != MessageBoxResult.Yes)
            {
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(txtDescription.Text))
        {
            MessageBoxResult confirmDesc = MessageBox.Show(
                "Description is empty. Do you want to continue?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmDesc != MessageBoxResult.Yes)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearInputs()
    {
        _isInternalChange = true;
        txtProjectId.Clear();
        txtProjectName.Clear();
        dpStartDate.SelectedDate = null;
        cboType.SelectedItem = null;
        cboDepartment.SelectedValue = null;
        txtDescription.Clear();
        _isInternalChange = false;
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ValidateInput())
            {
                return;
            }

            if (!ConfirmOptionalFields())
            {
                return;
            }

            using var context = new Assigment2DbContext();
            var newProject = new Project
            {
                Name = txtProjectName.Text.Trim(),
                StartDate = DateOnly.FromDateTime(dpStartDate.SelectedDate!.Value),
                Type = cboType.SelectedItem!.ToString(),
                Department = (int?)cboDepartment.SelectedValue,
                Description = txtDescription.Text.Trim()
            };

            context.Projects.Add(newProject);
            context.SaveChanges();

            LoadProjects();
            ClearInputs();
            MessageBox.Show("Add Successful", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgProjects.SelectedItem is Project selectedProject)
        {
            _isInternalChange = true;
            txtProjectId.Text = selectedProject.Id.ToString();
            txtProjectName.Text = selectedProject.Name;
            dpStartDate.SelectedDate = selectedProject.StartDate.HasValue
                ? selectedProject.StartDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;
            cboType.SelectedItem = selectedProject.Type;
            cboDepartment.SelectedValue = selectedProject.Department;
            txtDescription.Text = selectedProject.Description ?? string.Empty;
            _isInternalChange = false;
        }
    }

    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtProjectId.Text) || !int.TryParse(txtProjectId.Text, out int projectId))
            {
                MessageBox.Show("Please select a project to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput())
            {
                return;
            }

            if (!ConfirmOptionalFields())
            {
                return;
            }

            using var context = new Assigment2DbContext();
            var existingProject = context.Projects.FirstOrDefault(p => p.Id == projectId);
            if (existingProject == null)
            {
                MessageBox.Show("Project not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            existingProject.Name = txtProjectName.Text.Trim();
            existingProject.StartDate = DateOnly.FromDateTime(dpStartDate.SelectedDate!.Value);
            existingProject.Type = cboType.SelectedItem!.ToString();
            existingProject.Department = (int?)cboDepartment.SelectedValue;
            existingProject.Description = txtDescription.Text.Trim();

            context.SaveChanges();
            LoadProjects();
            MessageBox.Show("Update Successful", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (dgProjects.SelectedItem is not Project && (string.IsNullOrWhiteSpace(txtProjectId.Text) || !int.TryParse(txtProjectId.Text, out _)))
            {
                MessageBox.Show("Please select a project to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int projectId = dgProjects.SelectedItem is Project selected
                ? selected.Id
                : int.Parse(txtProjectId.Text);

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete this project?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                using var context = new Assigment2DbContext();
                var projectToDelete = context.Projects.FirstOrDefault(p => p.Id == projectId);
                if (projectToDelete != null)
                {
                    context.Projects.Remove(projectToDelete);
                    context.SaveChanges();
                    LoadProjects();
                    LoadTypes();
                    ClearInputs();
                    MessageBox.Show("Delete Successful", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void cboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInternalChange || cboType.SelectedItem == null)
        {
            return;
        }

        string selectedType = cboType.SelectedItem.ToString()!;
        TypeProjectWindow typeWindow = new TypeProjectWindow(selectedType);
        typeWindow.ShowDialog();
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
