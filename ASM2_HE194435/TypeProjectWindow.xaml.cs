using System;
using System.Linq;
using System.Windows;
using ASM2_HE194435.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM2_HE194435;

public partial class TypeProjectWindow : Window
{
    private readonly string _selectedType;

    public TypeProjectWindow(string selectedType)
    {
        InitializeComponent();
        _selectedType = selectedType;
        LoadProjectsByType();
    }

    private void LoadProjectsByType()
    {
        try
        {
            using var context = new Assigment2DbContext();
            var filteredProjects = context.Projects
                .Include(p => p.DepartmentNavigation)
                .Where(p => p.Type == _selectedType)
                .ToList();
            dgTypeProjects.ItemsSource = filteredProjects;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading projects by type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
