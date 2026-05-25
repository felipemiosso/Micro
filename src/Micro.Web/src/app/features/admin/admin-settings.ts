import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AdminService, Department, SalaryBand, CostCenter } from './admin.service';
import { NotificationService } from '../../core/ui/notification.service';
import { MatIconModule } from '@angular/material/icon';
import { UserListComponent } from '../users/list/user-list';
import { RoleListComponent } from '../roles/list/role-list';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatIconModule, UserListComponent, RoleListComponent],
  template: `
    <div class="p-8 bg-hex-pattern min-h-screen">
      <div class="max-w-6xl mx-auto">
        <div class="flex justify-between items-end mb-10">
          <div>
            <h1 class="text-4xl font-bold text-ink tracking-tight mb-2">Admin Settings</h1>
            <p class="text-ink-light font-medium">Manage system lookup tables and configuration.</p>
          </div>
        </div>

        <div class="flex gap-1 bg-surface-alt p-1 rounded-xl w-fit mb-8 border border-slate-200 shadow-sm">
          <button 
            (click)="switchTab('departments')" 
            [class.bg-white]="activeTab() === 'departments'"
            [class.shadow-sm]="activeTab() === 'departments'"
            [class.text-primary]="activeTab() === 'departments'"
            class="px-6 py-2 rounded-lg text-sm font-bold transition-all text-slate-500 hover:text-ink">
            Departments
          </button>
          <button 
            (click)="switchTab('salary-bands')" 
            [class.bg-white]="activeTab() === 'salary-bands'"
            [class.shadow-sm]="activeTab() === 'salary-bands'"
            [class.text-primary]="activeTab() === 'salary-bands'"
            class="px-6 py-2 rounded-lg text-sm font-bold transition-all text-slate-500 hover:text-ink">
            Salary Bands
          </button>
          <button 
            (click)="switchTab('cost-centers')" 
            [class.bg-white]="activeTab() === 'cost-centers'"
            [class.shadow-sm]="activeTab() === 'cost-centers'"
            [class.text-primary]="activeTab() === 'cost-centers'"
            class="px-6 py-2 rounded-lg text-sm font-bold transition-all text-slate-500 hover:text-ink">
            Cost Centers
          </button>
          <button 
            (click)="switchTab('users')" 
            [class.bg-white]="activeTab() === 'users'"
            [class.shadow-sm]="activeTab() === 'users'"
            [class.text-primary]="activeTab() === 'users'"
            class="px-6 py-2 rounded-lg text-sm font-bold transition-all text-slate-500 hover:text-ink">
            Users
          </button>
          <button 
            (click)="switchTab('roles')" 
            [class.bg-white]="activeTab() === 'roles'"
            [class.shadow-sm]="activeTab() === 'roles'"
            [class.text-primary]="activeTab() === 'roles'"
            class="px-6 py-2 rounded-lg text-sm font-bold transition-all text-slate-500 hover:text-ink">
            Roles
          </button>
          <a 
            routerLink="/admin/custom-fields"
            class="px-6 py-2 rounded-lg text-sm font-bold transition-all text-slate-500 hover:text-ink inline-flex items-center">
            Custom Fields
          </a>
        </div>

        <ng-container *ngIf="activeTab() === 'users'">
          <app-user-list></app-user-list>
        </ng-container>

        <ng-container *ngIf="activeTab() === 'roles'">
          <app-role-list></app-role-list>
        </ng-container>

        <div *ngIf="['departments', 'salary-bands', 'cost-centers'].includes(activeTab())" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <!-- List Column -->
          <div class="lg:col-span-2 space-y-6">
            <div class="card-honeycomb overflow-hidden !p-0">
              <table class="w-full text-left border-collapse">
                <thead>
                  <tr class="bg-surface-alt border-b border-slate-200">
                    <ng-container [ngSwitch]="activeTab()">
                      <th *ngSwitchCase="'departments'" class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest">Department Name</th>
                      <th *ngSwitchCase="'salary-bands'" class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest">Band Name</th>
                      <th *ngSwitchCase="'cost-centers'" class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest">Code & Name</th>
                    </ng-container>
                    <th class="px-6 py-4 text-xs font-bold text-ink uppercase tracking-widest text-right">Actions</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100 bg-white">
                  <!-- Departments -->
                  <ng-container *ngIf="activeTab() === 'departments'">
                    <tr *ngFor="let d of departments()" class="hover:bg-honeycomb-50/30 transition-colors group">
                      <td class="px-6 py-4">
                        <div class="font-bold text-ink">{{ d.name }}</div>
                        <span [class]="d.isActive ? 'badge-green' : 'badge-gray'" class="text-[10px]">
                          {{ d.isActive ? 'Active' : 'Inactive' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 text-right">
                        <button (click)="setEdit('dept', d)" class="text-primary font-bold text-xs uppercase tracking-wider hover:underline">Edit</button>
                      </td>
                    </tr>
                  </ng-container>

                  <!-- Salary Bands -->
                  <ng-container *ngIf="activeTab() === 'salary-bands'">
                    <tr *ngFor="let b of salaryBands()" class="hover:bg-honeycomb-50/30 transition-colors group">
                      <td class="px-6 py-4">
                        <div class="font-bold text-ink">{{ b.name }}</div>
                        <div class="text-xs text-slate-500 font-medium">
                          {{ b.minAmount | number }} - {{ b.maxAmount | number }} {{ b.currency }}
                        </div>
                      </td>
                      <td class="px-6 py-4 text-right">
                        <button (click)="setEdit('band', b)" class="text-primary font-bold text-xs uppercase tracking-wider hover:underline">Edit</button>
                      </td>
                    </tr>
                  </ng-container>

                  <!-- Cost Centers -->
                  <ng-container *ngIf="activeTab() === 'cost-centers'">
                    <tr *ngFor="let cc of costCenters()" class="hover:bg-honeycomb-50/30 transition-colors group">
                      <td class="px-6 py-4">
                        <div class="font-mono text-xs font-black text-honeycomb-600 mb-1">{{ cc.code }}</div>
                        <div class="font-bold text-ink">{{ cc.name }}</div>
                      </td>
                      <td class="px-6 py-4 text-right">
                        <button (click)="setEdit('cc', cc)" class="text-primary font-bold text-xs uppercase tracking-wider hover:underline">Edit</button>
                      </td>
                    </tr>
                  </ng-container>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Editor Column -->
          <div class="lg:col-span-1">
            <div class="card-honeycomb sticky top-8">
              <h3 class="text-lg font-bold text-ink mb-6 flex items-center gap-2">
                <mat-icon class="text-primary">edit_note</mat-icon>
                {{ editingId() ? 'Edit' : 'Add New' }} 
                {{ activeTab() === 'departments' ? 'Department' : activeTab() === 'salary-bands' ? 'Salary Band' : 'Cost Center' }}
              </h3>

              <form (ngSubmit)="save()" class="space-y-5">
                <!-- Department Form -->
                <ng-container *ngIf="activeTab() === 'departments'">
                  <div class="flex flex-col gap-2">
                    <label class="text-[10px] font-black text-ink uppercase tracking-widest">Name</label>
                    <input type="text" [(ngModel)]="deptForm.name" name="name" class="input-honeycomb" placeholder="e.g. Engineering">
                  </div>
                  <div class="flex items-center gap-3 py-2">
                    <input type="checkbox" [(ngModel)]="deptForm.isActive" name="isActive" class="w-4 h-4 text-primary rounded border-slate-300">
                    <label class="text-xs font-bold text-ink">Active</label>
                  </div>
                </ng-container>

                <!-- Salary Band Form -->
                <ng-container *ngIf="activeTab() === 'salary-bands'">
                  <div class="flex flex-col gap-2">
                    <label class="text-[10px] font-black text-ink uppercase tracking-widest">Band Name</label>
                    <input type="text" [(ngModel)]="bandForm.name" name="name" class="input-honeycomb" placeholder="e.g. Senior Backend">
                  </div>
                  <div class="grid grid-cols-2 gap-4">
                    <div class="flex flex-col gap-2">
                      <label class="text-[10px] font-black text-ink uppercase tracking-widest">Min</label>
                      <input type="number" [(ngModel)]="bandForm.minAmount" name="min" class="input-honeycomb">
                    </div>
                    <div class="flex flex-col gap-2">
                      <label class="text-[10px] font-black text-ink uppercase tracking-widest">Max</label>
                      <input type="number" [(ngModel)]="bandForm.maxAmount" name="max" class="input-honeycomb">
                    </div>
                  </div>
                  <div class="flex flex-col gap-2">
                    <label class="text-[10px] font-black text-ink uppercase tracking-widest">Currency</label>
                    <input type="text" [(ngModel)]="bandForm.currency" name="currency" class="input-honeycomb" placeholder="USD">
                  </div>
                </ng-container>

                <!-- Cost Center Form -->
                <ng-container *ngIf="activeTab() === 'cost-centers'">
                  <div class="flex flex-col gap-2">
                    <label class="text-[10px] font-black text-ink uppercase tracking-widest">Code</label>
                    <input type="text" [(ngModel)]="ccForm.code" name="code" class="input-honeycomb" placeholder="e.g. RD-100">
                  </div>
                  <div class="flex flex-col gap-2">
                    <label class="text-[10px] font-black text-ink uppercase tracking-widest">Name</label>
                    <input type="text" [(ngModel)]="ccForm.name" name="name" class="input-honeycomb" placeholder="e.g. Research & Dev">
                  </div>
                </ng-container>

                <div class="flex flex-col gap-3 pt-4">
                  <button type="submit" class="btn-primary w-full shadow-lg shadow-honeycomb-500/20">
                    {{ editingId() ? 'Update' : 'Create' }}
                  </button>
                  <button type="button" (click)="resetForm()" class="text-xs font-bold text-slate-400 hover:text-ink uppercase tracking-widest py-2">
                    Cancel / Reset
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminSettingsComponent implements OnInit {
  private adminService = inject(AdminService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  activeTab = signal<'departments' | 'salary-bands' | 'cost-centers' | 'users' | 'roles'>('departments');
  editingId = signal<string | null>(null);

  departments = signal<Department[]>([]);
  salaryBands = signal<SalaryBand[]>([]);
  costCenters = signal<CostCenter[]>([]);

  // Forms
  deptForm = { name: '', isActive: true };
  bandForm = { name: '', minAmount: 0, maxAmount: 0, currency: 'USD' };
  ccForm = { code: '', name: '' };

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab.set(params['tab'] as any);
      }
    });
    this.loadAll();
  }

  switchTab(tab: string) {
    this.router.navigate([], { queryParams: { tab } });
  }

  loadAll() {
    this.adminService.getDepartments().subscribe(data => this.departments.set(data));
    this.adminService.getSalaryBands().subscribe(data => this.salaryBands.set(data));
    this.adminService.getCostCenters().subscribe(data => this.costCenters.set(data));
  }

  setEdit(type: string, data: any) {
    this.editingId.set(data.id);
    if (type === 'dept') this.deptForm = { ...data };
    if (type === 'band') this.bandForm = { ...data };
    if (type === 'cc') this.ccForm = { ...data };
  }

  resetForm() {
    this.editingId.set(null);
    this.deptForm = { name: '', isActive: true };
    this.bandForm = { name: '', minAmount: 0, maxAmount: 0, currency: 'USD' };
    this.ccForm = { code: '', name: '' };
  }

  save() {
    const tab = this.activeTab();
    const id = this.editingId();

    if (tab === 'departments') {
      if (id) {
        this.adminService.updateDepartment(id, this.deptForm).subscribe(() => { this.notify.success('Department saved'); this.loadAll(); this.resetForm(); });
      } else {
        this.adminService.createDepartment(this.deptForm).subscribe(() => { this.notify.success('Department saved'); this.loadAll(); this.resetForm(); });
      }
    } else if (tab === 'salary-bands') {
      if (id) {
        this.adminService.updateSalaryBand(id, this.bandForm).subscribe(() => { this.notify.success('Salary band saved'); this.loadAll(); this.resetForm(); });
      } else {
        this.adminService.createSalaryBand(this.bandForm).subscribe(() => { this.notify.success('Salary band saved'); this.loadAll(); this.resetForm(); });
      }
    } else if (tab === 'cost-centers') {
      if (id) {
        this.adminService.updateCostCenter(id, this.ccForm).subscribe(() => { this.notify.success('Cost center saved'); this.loadAll(); this.resetForm(); });
      } else {
        this.adminService.createCostCenter(this.ccForm).subscribe(() => { this.notify.success('Cost center saved'); this.loadAll(); this.resetForm(); });
      }
    }
  }
}
