import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { EventListComponent } from './views/external/event-list.component';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet, EventListComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('app');
}
