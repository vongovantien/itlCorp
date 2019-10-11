import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginationModule, TabsModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { GroupComponent } from './group.component';
import { AddGroupComponent } from './add/add-group/add-group.component';
import { GroupDetailComponent } from './detail/detail-group/detail-group.component';
import { FormSearchGroupComponent } from './components/form-search-group/form-search-group.component';


const routing: Routes = [
    {
        path: "", component: GroupComponent, pathMatch: 'full',
        data: { name: "Group", path: "group", level: 2 }
    },
    {
        path: "new", component: AddGroupComponent,
        data: { name: "New", path: "New", level: 3 }
    },
    {
        path: ":id", component: GroupDetailComponent,
        data: { name: "Detail", path: "Detail", level: 3 }
    }
];

@NgModule({
    declarations: [
        GroupComponent,
        AddGroupComponent,
        GroupDetailComponent,
        FormSearchGroupComponent
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
        GroupComponent,
        AddGroupComponent,
        GroupDetailComponent,
        FormSearchGroupComponent
    ]
})
export class GroupModule { }