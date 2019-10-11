import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginationModule, TabsModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { GroupComponent } from './group.component';


const routing: Routes = [
    {
        path: "", component: GroupComponent, pathMatch: 'full',
        data: { name: "Group", path: "group", level: 2 }
    }
];

@NgModule({
    declarations: [
        GroupComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        TabsModule.forRoot(),
        PaginationModule.forRoot(),
        ReactiveFormsModule,
        PerfectScrollbarModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
    bootstrap: [
        GroupComponent
    ]
})
export class GroupModule { }