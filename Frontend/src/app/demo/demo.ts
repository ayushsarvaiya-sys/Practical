import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DemoService } from '../Services/demo-service';

@Component({
  selector: 'app-demo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './demo.html',
  styleUrl: './demo.css',
})
export class Demo implements OnInit {
  weatherData = signal<any[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private demoService: DemoService) {}

  ngOnInit() {
    this.fetchWeatherForecast();
  }

  fetchWeatherForecast() {
    this.loading.set(true);
    this.error.set(null);

    this.demoService.getWeatherForecast().subscribe({
      next: (data) => {
        this.weatherData.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load weather forecast: ' + err.message);
        this.loading.set(false);
      },
    });
  }
}
